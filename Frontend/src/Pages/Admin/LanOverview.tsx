import styled from "@emotion/styled"
import {
  Add,
  Edit,
  Upload,
  Fullscreen,
  Delete,
  Cancel,
} from "@mui/icons-material"
import {
  Stack,
  Typography,
  Button,
  Accordion,
  AccordionSummary,
  Divider,
  AccordionDetails,
  TableContainer,
  Table,
  TableBody,
  TableRow,
  TableCell,
  IconButton,
  FormControlLabel,
  Switch,
  Modal,
  Box,
  TextField,
} from "@mui/material"
import { useState } from "react"
import { useLanAdapter, Lan, useLans } from "../../Adapters/LanAdapter"
import DelayedCircularProgress from "../../Components/DelayedCircularProgress"
import { useAlerts } from "../../Contexts/AlertContext"
import { Guild } from "../../Adapters/GuildAdapter"

function GetFirstLanOrNull(lans: Lan[] | null) {
  return lans && lans.length > 0 ? lans[0] : null
}

function getSelectedGuildLans(lans: Lan[] | null, guildId: string) {
  return lans && lans.filter((lan) => lan.guildId == guildId)
}

export default function LanOverview(props: { guild: Guild }) {
  const lans = useLans()
  const selectedGuildLans = getSelectedGuildLans(lans, props.guild.id)
  const [selectedLan, setSelectedLan] = useState<Lan | null>(
    GetFirstLanOrNull(selectedGuildLans)
  )

  function handleLanSelected(lan: Lan) {
    setSelectedLan(selectedLan?.id == lan.id ? null : lan)
  }

  return (
    <Stack width={"100%"}>
      <LanListHeader guild={props.guild} />
      {selectedGuildLans?.length ? (
        <LanList
          lans={selectedGuildLans}
          selectedLan={selectedLan}
          onLanSelected={handleLanSelected}
        />
      ) : selectedGuildLans ? (
        <NoLanExists />
      ) : (
        <Stack width="100%" justifyContent="center" alignItems="center">
          <DelayedCircularProgress />
        </Stack>
      )}
    </Stack>
  )
}

function LanListHeader(props: { guild: Guild }) {
  const { alertSuccess, alertLoading } = useAlerts()
  const { createLan } = useLanAdapter()
  const [createLanVisibility, setCreateVisibility] = useState<boolean>(false)

  function handleCancelClicked() {
    setCreateVisibility(false)
  }

  async function handleCreatePressed(title: string, background: string) {
    await alertLoading("Oppretter " + title, async () => {
      await createLan(props.guild.id, title, background)
    })
    alertSuccess(title + " har blitt opprettet")
    setCreateVisibility(false)
  }

  return (
    <Stack width={"100%"} spacing={2}>
      <Stack direction="row" width="100%" justifyContent="space-between">
        <Typography variant="h4">{props.guild.name}</Typography>
        <Button
          onClick={() => setCreateVisibility(true)}
          startIcon={<Add />}
          variant="outlined"
          color="secondary"
        >
          Nytt lan
        </Button>
      </Stack>
      {createLanVisibility && (
        <LanEditor
          onSaveClicked={(title, background) =>
            handleCreatePressed(title, background)
          }
          onCancelClicked={handleCancelClicked}
        />
      )}
    </Stack>
  )
}

function LanList(props: {
  lans: Lan[]
  selectedLan: Lan | null
  onLanSelected: (lan: Lan) => void
}) {
  const { updateLan } = useLanAdapter()
  const [viewEditor, setViewEditor] = useState<boolean>(false)

  function handleEditClicked() {
    setViewEditor(true)
  }

  async function handleSaveClicked(
    lan: Lan,
    title: string,
    background: string
  ) {
    await updateLan(lan, title, background)
    setViewEditor(false)
  }

  function handleCancelClicked() {
    setViewEditor(false)
  }

  return (
    <Stack
      justifyContent="center"
      alignItems="center"
      sx={{ marginTop: "1em" }}
      width="100%"
    >
      {props.lans.map((lan) => (
        <Accordion
          key={lan.id}
          expanded={props.selectedLan != null && props.selectedLan.id == lan.id}
          onChange={() => props.onLanSelected(lan)}
          sx={{ width: "100%" }}
        >
          <AccordionSummary sx={{ width: "100%" }}>
            <Stack
              width="100%"
              direction="row"
              spacing={2}
              justifyContent="space-between"
            >
              <Typography>{lan.title}</Typography>
              <Typography color={"textSecondary"} variant="subtitle2">
                {lan.active ? "Aktiv" : "Inaktiv"}
              </Typography>
            </Stack>
          </AccordionSummary>
          <Divider orientation="horizontal" flexItem />
          <AccordionDetails sx={{ paddingTop: "1em" }}>
            <Stack
              width="100%"
              spacing={2}
              justifyContent="space-between"
              alignItems="center"
            >
              {viewEditor ? (
                <LanEditor
                  lan={lan}
                  onSaveClicked={(title, background) =>
                    handleSaveClicked(lan, title, background)
                  }
                  onCancelClicked={handleCancelClicked}
                />
              ) : (
                <LanDetails lan={lan} onEditClick={handleEditClicked} />
              )}
            </Stack>
          </AccordionDetails>
        </Accordion>
      ))}
    </Stack>
  )
}

function LanDetails(props: { lan: Lan; onEditClick: (lan: Lan) => void }) {
  const { alertLoading, alertSuccess } = useAlerts()
  const { deleteLan, setActiveLan: setActiveGuildId } = useLanAdapter()
  const [viewBackground, setViewBackground] = useState<boolean>(false)

  function formatTime(date: Date) {
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

  async function handleDeleteClick(lan: Lan) {
    await alertLoading(`Sletter ${lan.title} ...`, async () => {
      await deleteLan(lan)
    })
    alertSuccess(`${lan.title} har blitt slettet`)
  }

  function handleActiveChange(lan: Lan) {
    setActiveGuildId(lan, !lan.active)
  }

  function handleEditClick(lan: Lan) {
    props.onEditClick(lan)
  }

  return (
    <Stack sx={{ width: "100%" }}>
      <TableContainer>
        <Table sx={{ width: "100%" }} size="small">
          <TableBody>
            {details.map((detail) => (
              <TableRow key={detail.label}>
                <TableCell component="th" scope="row">
                  {detail.label}
                </TableCell>
                <TableCell align="right">{detail.value}</TableCell>
              </TableRow>
            ))}
            <TableRow key={"background"}>
              <TableCell component="th" scope="row">
                Bakgrunn
              </TableCell>
              <TableCell align="right">
                <Button
                  variant="text"
                  size="small"
                  onClick={() => setViewBackground(true)}
                >
                  Vis bakgrunn
                </Button>
              </TableCell>
            </TableRow>
          </TableBody>
        </Table>
      </TableContainer>

      <Stack direction={"row"} justifyContent={"space-between"}>
        <IconButton onClick={() => handleEditClick(props.lan)}>
          <Edit />
        </IconButton>
        <FormControlLabel
          control={
            <Switch
              onChange={() => handleActiveChange(props.lan)}
              checked={props.lan.active}
              color="success"
            />
          }
          label="Aktiv"
        />
        <IconButton color="error" onClick={() => handleDeleteClick(props.lan)}>
          <Delete />
        </IconButton>
      </Stack>

      <BackgroundPreviewModal
        background={props.lan.background}
        open={viewBackground}
        onClose={() => setViewBackground(false)}
      />
    </Stack>
  )
}

function BackgroundPreviewModal(props: {
  background: string
  open: boolean
  onClose: () => void
}) {
  const base64ToBlob = (base64: string): Blob => {
    return new Blob([atob(base64)], { type: "image/svg+xml" })
  }

  return (
    <Modal {...props}>
      <Box
        sx={{
          position: "absolute",
          top: "50%",
          left: "50%",
          transform: "translate(-50%, -50%)",
          width: "60%",
          bgcolor: "background.paper",
          p: 4,
        }}
      >
        <img
          alt="preview"
          style={{ width: "100%" }}
          src={URL.createObjectURL(base64ToBlob(props.background))}
        />
      </Box>
    </Modal>
  )
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

function LanEditor(props: {
  lan?: Lan
  onSaveClicked: (title: string, background: string) => void
  onCancelClicked: () => void
}) {
  const [title, setTitle] = useState<string | null>(
    props.lan == undefined ? null : props.lan.title
  )
  const [background, setBackground] = useState<string | null>(
    props.lan == undefined ? null : props.lan.background
  )
  const [backgroundName, setBackgroundName] = useState<string | null>(null)
  const [viewBackground, setViewBackground] = useState<boolean>(false)
  const [errors, setErrors] = useState<{ title: boolean; background: boolean }>(
    {
      title: false,
      background: false,
    }
  )

  const blobToBase64 = (blob: Blob): Promise<string> => {
    const reader = new FileReader()
    reader.readAsDataURL(blob)
    return new Promise<string>((resolve) => {
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
      props.onSaveClicked(title, background)
    }
  }

  function handlePreviewBackgroundClicked() {
    setViewBackground(true)
  }

  function handleBackgroundPreviewClosed() {
    setViewBackground(false)
  }

  function handleCancelClicked() {
    props.onCancelClicked()
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

  return (
    <Stack
      spacing={2}
      justifyContent="center"
      alignItems="center"
      sx={{ width: "100%" }}
    >
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
        color={(errors.background && "error") || "primary"}
        component="label"
        variant="outlined"
        startIcon={<Upload />}
      >
        <Typography noWrap={true}>
          {backgroundName || "Last opp bakgrunn"}
        </Typography>
        <VisuallyHiddenInput
          type="file"
          onInput={handleBackgroundChanged}
          accept="image/svg"
        />
      </Button>
      {background && (
        <Button
          onClick={handlePreviewBackgroundClicked}
          startIcon={<Fullscreen />}
        >
          Forhåndsvis bakgrunn
        </Button>
      )}
      {background && (
        <BackgroundPreviewModal
          open={viewBackground}
          onClose={handleBackgroundPreviewClosed}
          background={background}
        />
      )}
      <Stack
        sx={{ width: "100%" }}
        spacing={1}
        direction="row"
        justifyContent="space-between"
        alignItems="center"
      >
        <Button
          onClick={handleSaveClicked}
          color="secondary"
          variant="contained"
        >
          Lagre
        </Button>
        <IconButton aria-label="cancel" onClick={handleCancelClicked}>
          <Cancel />
        </IconButton>
      </Stack>
    </Stack>
  )
}

function NoLanExists() {
  return <Typography color={"text.secondary"}>Ingen lan finnes</Typography>
}
