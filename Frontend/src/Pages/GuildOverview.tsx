/* eslint-disable @typescript-eslint/no-unused-vars */
import {Add, Delete, Edit, Fullscreen, Group, Upload} from "@mui/icons-material"
import {
  Accordion,
  AccordionDetails,
  AccordionSummary,
  Button,
  FormControlLabel,
  IconButton,
  Stack,
  styled,
  Switch,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableRow,
  TextField,
} from "@mui/material"
import Divider from "@mui/material/Divider"
import Typography from "@mui/material/Typography"
import {useState} from "react"
import {useAllLans} from "../Adapters/Lans/AllLans"
import {Guild, Lan} from "../Adapters/Models"
import Modal from "../Components/Modal"
import {useAlerts} from "../Contexts/AlertContext"
import {useGuild} from "../Adapters/Guilds/Guilds";
import {useNavigate} from "react-router-dom";

export default function GuildOverview(props: { guildId: string}) {
  const { alertLoading, alertSuccess } = useAlerts()
  const { guild } = useGuild(props.guildId)
  const navigate = useNavigate()
  const { lans, updateLan, createLan, deleteLan } = useAllLans(guild.id)
  const [previewBackground, setPreviewBackground] = useState<string | null>(null)
  const [editingLan, setEditingLan] = useState<Lan | null>(null)
  const [selectedLan, setSelectedLan] = useState<Lan | null>(null)
  const [showCreateLan, setShowCreateLan] = useState<boolean>(false)

  function handleEditLanClose() {
    setEditingLan(null)
  }

  function handleLanEditClick(lan: Lan) {
    setEditingLan(lan)
  }

  async function handleDeleteClick(lan: Lan) {
    await alertLoading(`Sletter ${lan.title} ...`, async () => {
      await deleteLan(lan)
    })
    alertSuccess(`${lan.title} har blitt slettet`)
  }

  async function handleActiveToggleClick(lan: Lan) {
    const activeString = lan.active ? "inaktiv" : "aktiv"
    await alertLoading(`Endrer ${lan.title} til ${activeString}`, async () => {
      await updateLan(lan, !lan.active, lan.title, lan.background)
    })
    alertSuccess(`${lan.title} ble satt til ${activeString}`)
  }

  async function handleSaveEditedLanClick(lan: Lan, title: string, background: string) {
    await alertLoading(`Oppdaterer ${lan.title}`, async () => {
      await updateLan(lan, lan.active, title, background)
    })
    alertSuccess(`${lan.title} ble oppdatert`)
    setEditingLan(null)
  }

  function handleLanCreateClick() {
    setShowCreateLan(true)
  }

  async function handleCreateLanClick(title: string, background: string) {
    await alertLoading("Oppretter " + title, async () => {
      await createLan(title, background)
    })
    alertSuccess(title + " har blitt opprettet")
    handleCreateLanClose()
  }

  function handleOnLanClick(lan: Lan) {
    setSelectedLan(selectedLan?.id == lan.id ? null : lan)
  }
  
  function handleRolesClick() {
    navigate(`/guild/${guild.id}/roles`)
  }

  function handlePreviewBackgrundClick(background: string) {
    setPreviewBackground(background)
  }

  function handlePreviewBackgrundClose() {
    setPreviewBackground(null)
  }

  function handleCreateLanClose() {
    setShowCreateLan(false)
  }

  return <Stack spacing={2} justifyContent="center" alignItems="center">
    <Stack width={"100%"}>
      <LanListHeader guild={guild} onCreateClick={handleLanCreateClick} onRolesClick={handleRolesClick} />
      <Stack justifyContent="center" alignItems="center" sx={{ marginTop: "1em" }} width="100%">
        {lans.map(lan => <LanDetails
          key={lan.id}
          lan={lan}
          selected={selectedLan?.id == lan.id}
          onEditClick={() => handleLanEditClick(lan)}
          onClick={() => handleOnLanClick(lan)}
          onActiveToggleClick={() => handleActiveToggleClick(lan)}
          onDeleteClick={() => handleDeleteClick(lan)}
          onPreviewBackgroundClick={() => handlePreviewBackgrundClick(lan.background)}
        />)}
        {lans.length == 0 && <NoLanExists />}
      </Stack>
    </Stack>

    {editingLan && <LanEditorModal
        open={Boolean(editingLan)}
        lan={editingLan}
        onClose={handleEditLanClose}
        onPreviewBackgroundClick={(background: string) => handlePreviewBackgrundClick(background)}
        onSaveClick={(title: string, background: string) =>
          handleSaveEditedLanClick(editingLan, title, background)
        }
      />}
    {showCreateLan && <LanEditorModal
        open={showCreateLan}
        onClose={handleCreateLanClose}
        onPreviewBackgroundClick={handlePreviewBackgrundClick}
        onSaveClick={handleCreateLanClick}
      />}
    {previewBackground && <BackgroundPreviewModal
        background={previewBackground}
        open={Boolean(previewBackground)}
        onClose={handlePreviewBackgrundClose}
      />}
  </Stack>
}

function LanListHeader(props: { guild: Guild; onCreateClick: () => void; onRolesClick: () => void }) {
  return <Stack width={"100%"} spacing={2}>
    <Stack direction="row" width="100%" justifyContent="space-between">
      <Typography variant="h4">{props.guild.name}</Typography>
      <Button
        onClick={() => props.onRolesClick()}
        startIcon={<Group />}
        variant="outlined"
      >
        Grupper
      </Button>
      <Button
        onClick={() => props.onCreateClick()}
        startIcon={<Add />}
        variant="outlined"
        color="secondary"
      >
        Nytt lan
      </Button>
    </Stack>
  </Stack>
}

function LanDetails(props: {
  lan: Lan
  selected: boolean
  onClick: () => void
  onEditClick: () => void
  onPreviewBackgroundClick: () => void
  onDeleteClick: () => void
  onActiveToggleClick: () => void
}) {
  const formatTime = (date: Date) => {
    return date.toLocaleTimeString("no", {
      day: "2-digit",
      month: "2-digit",
      year: "numeric",
      hour: "2-digit",
      minute: "2-digit",
      second: "2-digit",
    })
  }

  const details = [
    { label: "Id", value: props.lan.id },
    { label: "Navn", value: props.lan.title },
    { label: "Opprettet", value: formatTime(props.lan.createdAt) },
    { label: "Oppdatert", value: formatTime(props.lan.updatedAt) },
  ]

  const Summary = () => <AccordionSummary sx={{ width: "100%" }}>
    <Stack width="100%" direction="row" spacing={2} justifyContent="space-between">
      <Typography>{props.lan.title}</Typography>
      <Typography color={"textSecondary"} variant="subtitle2">
        {props.lan.active ? "Aktiv" : "Inaktiv"}
      </Typography>
    </Stack>
  </AccordionSummary>

  const Details = () => <TableContainer>
    <Table sx={{ width: "100%" }} size="small">
      <TableBody>
        {details.map(detail => <TableRow key={detail.label}>
          <TableCell component="th" scope="row">
            {detail.label}
          </TableCell>
          <TableCell align="right">{detail.value}</TableCell>
        </TableRow>)}
        <TableRow key={"background"}>
          <TableCell component="th" scope="row">
            Bakgrunn
          </TableCell>
          <TableCell align="right">
            <Button variant="text" size="small" onClick={() => props.onPreviewBackgroundClick()}>
              Vis bakgrunn
            </Button>
          </TableCell>
        </TableRow>
      </TableBody>
    </Table>
  </TableContainer>

  const Actions = () => <Stack direction={"row"} justifyContent={"space-between"}>
    <IconButton onClick={() => props.onEditClick()}>
      <Edit />
    </IconButton>
    <FormControlLabel
      control={
        <Switch
          onChange={() => props.onActiveToggleClick()}
          checked={props.lan.active}
          color="success"
        />
      }
      label="Aktiv"
    />
    <IconButton color="error" onClick={() => props.onDeleteClick()}>
      <Delete />
    </IconButton>
  </Stack>

  return <Accordion
    key={props.lan.id}
    expanded={props.selected}
    onChange={() => props.onClick()}
    sx={{ width: "100%" }}
  >
    <Summary />
    <Divider orientation="horizontal" flexItem />
    <AccordionDetails sx={{ paddingTop: "1em" }}>
      <Stack sx={{ width: "100%" }}>
        <Details />
        <Actions />
      </Stack>
    </AccordionDetails>
  </Accordion>
}

function BackgroundPreviewModal(props: { background: string; open: boolean; onClose: () => void }) {
  return <Modal title="Background preview" {...props}>
    <img
      alt="preview"
      style={{ width: "100%" }}
      src={`data:image/svg+xml;base64,${props.background}`}
    />
  </Modal>
}

const VisuallyHiddenInput = styled("input")({
  clip: "rect(0 0 0 0)",
  clipPath: "inset(50%)",
  height: 1,
  overflow: "hidden",
  position: "absolute",
  bottom: 0,
  left: 0,
  whiteSpace: "nowrap",
  width: 1,
})

function LanEditorModal(props: {
  lan?: Lan
  open: boolean
  onClose: () => void
  onPreviewBackgroundClick: (background: string) => void
  onSaveClick: (title: string, background: string) => void
}) {
  const [title, setTitle] = useState<string | null>(props.lan == undefined ? null : props.lan.title)
  const [background, setBackground] = useState<string | null>(
    props.lan == undefined ? null : props.lan.background
  )
  const [backgroundName, setBackgroundName] = useState<string | null>(null)
  const [errors, setErrors] = useState<{ title: boolean; background: boolean }>({
    title: false,
    background: false,
  })

  const blobToBase64 = (blob: Blob): Promise<string> => {
    const reader = new FileReader()
    reader.readAsDataURL(blob)
    return new Promise<string>(resolve => {
      reader.onloadend = () => {
        const result = reader.result as string
        if (result != null) {
          resolve(result.split(",")[1])
        }
      }
    })
  }

  async function handleSaveClicked() {
    setErrors({
      title: title == null || title.length == 0,
      background: background == null || background.length == 0,
    })

    if (title != null && background != null) {
      props.onSaveClick(title, background)
    }
  }

  function handleTitleChanged(event: React.ChangeEvent<HTMLInputElement>) {
    setTitle(event.target.value)
  }

  function handleBackgroundChanged(event: React.ChangeEvent<HTMLInputElement>) {
    const files = event.target.files
    if (files != null) {
      const file = files[0]
      const reader = new FileReader()

      reader.onload = async function (e) {
        if (e.target != null) {
          const bytes = e.target.result as ArrayBuffer
          const blob = new Blob([bytes], { type: "image/svg+xml" })
          setBackground(await blobToBase64(blob))
          setBackgroundName(file.name)
        }
      }

      reader.readAsArrayBuffer(file)
    }
  }

  return <Modal title={props.lan?.title ?? "Opprett nytt lan"} {...props}>
    <Stack spacing={2} justifyContent="center" alignItems="center" sx={{ width: "100%" }}>
      <TextField
        defaultValue={title}
        sx={{ width: "100%" }}
        required
        label="Lan name"
        variant="outlined"
        onChange={handleTitleChanged}
        error={errors.title}
      />
      <Button
        sx={{ width: "100%" }}
        color={errors.background && "error" || "primary"}
        component="label"
        variant="outlined"
        startIcon={<Upload />}
      >
        <Typography noWrap={true}>{backgroundName || "Last opp bakgrunn"}</Typography>
        <VisuallyHiddenInput type="file" onInput={handleBackgroundChanged} accept="image/svg" />
      </Button>
      {background && <Button
          onClick={() => props.onPreviewBackgroundClick(background)}
          startIcon={<Fullscreen />}
        >
          Forhåndsvis bakgrunn
        </Button>}
      <Button onClick={handleSaveClicked} color="secondary" variant="contained">
        Lagre
      </Button>
    </Stack>
  </Modal>
}

function NoLanExists() {
  return <Typography color={"text.secondary"}>Ingen lan finnes</Typography>
}
