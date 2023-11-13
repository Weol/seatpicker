import * as React from "react"
import {
  Accordion,
  AccordionDetails,
  AccordionSummary,
  List,
  ListItem,
  Paper,
  Stack,
} from "@mui/material"
import CancelIcon from "@mui/icons-material/Cancel"
import DeleteIcon from "@mui/icons-material/Delete"
import Button from "@mui/material/Button"
import Typography from "@mui/material/Typography"
import Divider from "@mui/material/Divider"
import { useState } from "react"
import { useAlerts } from "../../Contexts/AlertContext"
import { Add } from "@mui/icons-material"
import DelayedCircularProgress from "../../Components/DelayedCircularProgress"
import { useDialogs } from "../../Contexts/DialogContext"
import ListItemText from "@mui/material/ListItemText"
import ListItemIcon from "@mui/material/ListItemIcon"
import Avatar from "@mui/material/Avatar"
import Config from "../../config"
import LanEditor from "./LanEditor"
import { Lan, useLanAdapter } from "../../Adapters/LanAdapter"
import { Guild, useGuildAdapter } from "../../Adapters/GuildAdapter"

export default function Admin() {
  const { alertSuccess, alertLoading } = useAlerts()
  const { showDialog } = useDialogs()
  const { lans, reloadLans, createNewLan, deleteLan, updateLan } =
    useLanAdapter()
  const { guilds } = useGuildAdapter()
  const [isCreateVisible, setCreateVisible] = useState<boolean>(false)
  const [expandedLan, setExpandedLan] = useState<string | false>(false)
  const [selectedGuild, setSelectedGuild] = useState<string | null>(null)

  const handleLanPressed =
    (lan: string) => (event: React.SyntheticEvent, newExpanded: boolean) => {
      setExpandedLan(newExpanded ? lan : false)
    }

  const handleGuildPressed = (guild: Guild) => {
    setSelectedGuild(guild.id)
  }

  const onCreateLan = async (title: string, background: string) => {
    await alertLoading("Oppretter " + title, async () => {
      await createNewLan(title, background)
      await reloadLans()
    })
    setCreateVisible(false)
    alertSuccess(title + " har blitt opprettet")
  }

  const onUpdateLan = async (lan: Lan, title: string, background: string) => {
    await alertLoading("Oppdaterer " + title, async () => {
      await updateLan(lan, title, background)
      await reloadLans()
    })
    setExpandedLan(false)
    alertSuccess(title + " har blitt oppdatert")
  }

  const onCancelCreateLan = () => {
    setCreateVisible(false)
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
      <Paper sx={{ width: "100%" }}>
        <List component={"nav"}>
          {guilds?.map((guild) => (
            <ListItem
              onClick={() => handleGuildPressed(guild)}
              selected={selectedGuild == guild.id}
              key={guild.id}
            >
              <ListItemIcon>
                <Avatar
                  alt={guild.name}
                  src={
                    Config.DiscordGuildIconBaseUrl +
                      guild.id +
                      "/" +
                      guild.icon ?? "0"
                  }
                />
              </ListItemIcon>
              <ListItemText primary={guild.name} />
            </ListItem>
          ))}
        </List>
      </Paper>

      <Stack direction="row" width="100%" justifyContent="space-between">
        <Typography variant="h4">Alle lan</Typography>
        <Button
          onClick={() => setCreateVisible(true)}
          startIcon={<Add />}
          variant="outlined"
          color="secondary"
        >
          Nytt lan
        </Button>
      </Stack>
      {isCreateVisible && (
        <LanEditor
          backgroundButtonText={"Upload background"}
          saveButtonText={"Create"}
          cancelButtonIcon={<CancelIcon />}
          onSave={(title, background) => onCreateLan(title, background)}
          onCancel={onCancelCreateLan}
        />
      )}
      <Stack
        justifyContent="center"
        alignItems="center"
        sx={{ marginTop: "1em" }}
        width="100%"
      >
        {lans?.map((lan) => (
          <Accordion
            expanded={expandedLan == lan.id}
            onChange={handleLanPressed(lan.id)}
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
                <Typography color="textSecondary" variant="subtitle2">
                  {lan.createdAt.toLocaleDateString("no", {
                    day: "2-digit",
                    month: "2-digit",
                    year: "numeric",
                  })}
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
                <Typography color="textSecondary" variant="subtitle2">
                  {lan.id}
                </Typography>
                <LanEditor
                  lan={lan}
                  backgroundButtonText={"Change background"}
                  saveButtonText={"Save changes"}
                  cancelButtonIcon={<DeleteIcon />}
                  onSave={(title, background) =>
                    onUpdateLan(lan, title, background)
                  }
                  onCancel={() => onDeleteLan(lan)}
                />
              </Stack>
            </AccordionDetails>
          </Accordion>
        ))}
        {lans == null && <DelayedCircularProgress />}
      </Stack>
    </Stack>
  )
}
