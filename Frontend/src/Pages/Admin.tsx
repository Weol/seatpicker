import * as React from "react"
import {
  Accordion,
  AccordionDetails,
  AccordionSummary,
  Box,
  FormControlLabel,
  IconButton,
  List,
  ListItemButton,
  Modal,
  Paper,
  Stack,
  Switch,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableRow,
  TextField,
  styled,
} from "@mui/material"
import CancelIcon from "@mui/icons-material/Cancel"
import DeleteIcon from "@mui/icons-material/Delete"
import Button from "@mui/material/Button"
import Typography from "@mui/material/Typography"
import Divider from "@mui/material/Divider"
import { useEffect, useState } from "react"
import { useAlerts } from "../Contexts/AlertContext"
import {
  Add,
  Edit,
  Fullscreen,
  FullscreenExit,
  Upload,
} from "@mui/icons-material"
import DelayedCircularProgress from "../Components/DelayedCircularProgress"
import { useDialogs } from "../Contexts/DialogContext"
import ListItemText from "@mui/material/ListItemText"
import ListItemIcon from "@mui/material/ListItemIcon"
import { Lan, useLanAdapter } from "../Adapters/LanAdapter"
import { Guild, useGuildAdapter } from "../Adapters/GuildAdapter"
import DiscordGuildAvatar from "../Components/DiscordAvatar"

export default function Admin() {
  const { alertSuccess, alertLoading, alertError } = useAlerts()
  const { showDialog } = useDialogs()
  const { lans, reloadLans, createNewLan, deleteLan, updateLan, setLanActive } =
    useLanAdapter()
  const { guilds } = useGuildAdapter()
  const [selectedGuild, setSelectedGuild] = useState<Guild | null>(null)

  const selectedGuildLans =
    lans && lans.filter((lan) => lan.guildId == selectedGuild?.id)

  useEffect(() => {
    setSelectedGuild(guilds?.length ? guilds[0] : null)
  }, [guilds])

  const onCreateLan = async (
    guild: Guild,
    title: string,
    background: string
  ) => {
    if (selectedGuild == null) {
      alertError("No guild is selected")
    } else {
      await alertLoading("Oppretter " + title, async () => {
        await createNewLan(guild.id, title, background)
        await reloadLans()
      })
      alertSuccess(title + " har blitt opprettet")
    }
  }

  const onUpdateLan = async (lan: Lan, title: string, background: string) => {
    await alertLoading("Oppdaterer " + title, async () => {
      await updateLan(lan, title, background)
      await reloadLans()
    })
    alertSuccess(title + " har blitt oppdatert")
  }

  const onActiveChange = async (lan: Lan, active: boolean) => {
    await alertLoading(`Setter ${lan.title} til aktiv`, async () => {
      await setLanActive(lan, active)
      await reloadLans()
    })
    alertSuccess(lan.title + " har blitt aktic")
  }

  const onDeleteLan = async (lan: Lan) => {
    const result = await showDialog<Lan>(
      "Er du sikker?",
      `Er du sikker på at du vil slette lanet "${lan.title}"? Denne operasjonen kan ikke angres.`,
      lan,
      "Ja",
      "Nei"
    )

    if (result.positive) {
      await alertLoading("Sletter " + result.metadata.title, async () => {
        await deleteLan(lan)
        await reloadLans()
      })

      alertSuccess(lan.title + " har blitt slettet")
    }
  }

  return (
    <Stack
      divider={<Divider orientation="horizontal" flexItem />}
      spacing={2}
      justifyContent="center"
      alignItems="center"
      sx={{ marginTop: "1em" }}
    >
      <Typography variant="h4">Alle servere</Typography>
      {guilds && selectedGuild ? (
        <GuildList
          guilds={guilds}
          selectedGuild={selectedGuild}
          onGuildSelected={setSelectedGuild}
        />
      ) : (
        <DelayedCircularProgress />
      )}
      {selectedGuild && (
        <LanListHeader guild={selectedGuild} onCreateLan={onCreateLan} />
      )}

      {selectedGuildLans?.length ? (
        <LanList
          lans={selectedGuildLans}
          onDeleteLan={onDeleteLan}
          onUpdateLan={onUpdateLan}
          onActiveChange={onActiveChange}
        />
      ) : selectedGuild ? (
        <NoLanExists />
      ) : (
        <DelayedCircularProgress />
      )}
    </Stack>
  )
}

function GuildList(props: {
  guilds: Guild[]
  selectedGuild: Guild
  onGuildSelected: (guild: Guild) => void
}) {
  return (
    <Paper sx={{ width: "100%" }}>
      <List component={"nav"}>
        {props.guilds.map((guild) => (
          <ListItemButton
            onClick={() => props.onGuildSelected(guild)}
            selected={props.selectedGuild.id == guild.id}
            key={guild.id}
          >
            <ListItemIcon>
              <DiscordGuildAvatar guild={guild} />
            </ListItemIcon>
            <ListItemText primary={guild.name} />
          </ListItemButton>
        ))}
      </List>
    </Paper>
  )
}

function LanListHeader(props: {
  guild: Guild
  onCreateLan: (
    guild: Guild,
    title: string,
    background: string
  ) => Promise<void>
}) {
  const [createLanVisibility, setCreateVisibility] = useState<boolean>(false)

  const handleCancelPressed = () => {
    setCreateVisibility(false)
  }

  const handleCreatePressed = async (title: string, background: string) => {
    await props.onCreateLan(props.guild, title, background)
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
          onSave={(title, background) => handleCreatePressed(title, background)}
          onCancel={handleCancelPressed}
        />
      )}
    </Stack>
  )
}

function LanList(props: {
  lans: Lan[]
  onDeleteLan: (lan: Lan) => Promise<void>
  onActiveChange: (lan: Lan, active: boolean) => Promise<void>
  onUpdateLan: (
    lan: Lan,
    newTitle: string,
    newBackground: string
  ) => Promise<void>
}) {
  const [selectedLan, setSelectedLan] = useState<Lan | null>(null)
  const [viewEdit, setViewEdit] = useState<boolean>(false)

  const onSave = async (lan: Lan, title: string, background: string) => {
    await props.onUpdateLan(lan, title, background)
    setViewEdit(false)
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
          expanded={selectedLan?.id == lan.id}
          onChange={() =>
            setSelectedLan(lan.id == selectedLan?.id ? null : lan)
          }
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
              {viewEdit ? (
                <LanEditor
                  lan={lan}
                  onSave={(title, background) => onSave(lan, title, background)}
                  onCancel={() => setViewEdit(false)}
                />
              ) : (
                <LanDetails
                  lan={lan}
                  onDelete={props.onDeleteLan}
                  onEdit={() => setViewEdit(true)}
                  onActiveChange={props.onActiveChange}
                />
              )}
            </Stack>
          </AccordionDetails>
        </Accordion>
      ))}
    </Stack>
  )
}

function LanDetails(props: {
  lan: Lan
  onDelete: (lan: Lan) => Promise<void>
  onEdit: (lan: Lan) => void
  onActiveChange: (lan: Lan, active: boolean) => Promise<void>
}) {
  const [viewBackground, setViewBackground] = useState<boolean>(false)
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
        <IconButton onClick={() => props.onEdit(props.lan)}>
          <Edit />
        </IconButton>
        <FormControlLabel
          control={
            <Switch
              onChange={() =>
                props.onActiveChange(props.lan, !props.lan.active)
              }
              checked={props.lan.active}
              color="success"
            />
          }
          label="Aktiv"
        />
        <IconButton color="error" onClick={() => props.onDelete(props.lan)}>
          <DeleteIcon />
        </IconButton>
      </Stack>

      <BackgroundModal
        background={props.lan.background}
        open={viewBackground}
        onClose={() => setViewBackground(false)}
      />
    </Stack>
  )
}

function BackgroundModal(props: {
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
  onSave: (title: string, background: string) => Promise<void>
  onCancel: () => void
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

  const onSavePressed = async () => {
    setErrors({
      title: title == null || title.length == 0,
      background: background == null || background.length == 0,
    })

    if (title != null && background != null) {
      props.onSave(title, background)
    }
  }

  const onTitleChanged = (event: React.ChangeEvent<HTMLInputElement>) => {
    setTitle(event.target.value)
  }

  const onBackgroundChanged = (event: React.ChangeEvent<HTMLInputElement>) => {
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
        onChange={onTitleChanged}
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
          onInput={onBackgroundChanged}
          accept="image/svg"
        />
      </Button>
      {background && (
        <Button
          onClick={() => setViewBackground(!viewBackground)}
          startIcon={viewBackground ? <FullscreenExit /> : <Fullscreen />}
        >
          {viewBackground ? "Gjem forhåndsvisning" : "Forhåndsvis bakgrunn"}
        </Button>
      )}
      {background && (
        <BackgroundModal
          open={viewBackground}
          onClose={() => setViewBackground(false)}
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
        <Button onClick={onSavePressed} color="secondary" variant="contained">
          Lagre
        </Button>
        <IconButton aria-label="cancel" onClick={props.onCancel}>
          <CancelIcon />
        </IconButton>
      </Stack>
    </Stack>
  )
}

function NoLanExists() {
  return <Typography color={"text.secondary"}>Ingen lan finnes</Typography>
}
