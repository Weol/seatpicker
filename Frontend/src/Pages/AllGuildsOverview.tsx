/* eslint-disable @typescript-eslint/no-unused-vars */
import {Add, Cancel, Delete, Edit, Save} from "@mui/icons-material"
import {
  Button,
  Divider,
  IconButton,
  List,
  ListItemButton,
  ListItemSecondaryAction,
  Paper,
  Stack,
  TextField,
  Typography,
} from "@mui/material"
import ListItemIcon from "@mui/material/ListItemIcon"
import ListItemText from "@mui/material/ListItemText"
import React, {useState} from "react"
import {useGuilds, useUnconfiguredGuilds} from "../Adapters/Guilds/Guilds"
import {Guild, UnconfiguredGuild} from "../Adapters/Models"
import DiscordGuildAvatar from "../Components/DiscordAvatar"
import {useNavigate} from "react-router-dom"
import {BadRequestError, ConflictError} from "../Adapters/AdapterError";
import {useAlerts} from "../Contexts/AlertContext";

export default function AllGuildsOverview() {
  const { alertError, alertLoading, alertSuccess } = useAlerts()
  const { configuredGuilds , updateGuild } = useGuilds()
  const { unconfiguredGuilds, configureGuild } = useUnconfiguredGuilds()
  const [selectedGuildId, setSelectedGuildId] = useState<string | null>(null)
  const navigate = useNavigate()
  const selectedGuild = configuredGuilds.find(guild => guild.id === selectedGuildId) ?? null

  function handleGuildSelect(guild: Guild) {
    setSelectedGuildId(guild.id)
  }

  async function handleHostAdded(guild: Guild, host: string) {
    const updatedHostNames = [...guild.hostnames, host]
    try {
      await alertLoading("Legger til host", async () => {
        await updateGuild({... guild, hostnames: updatedHostNames })
      })
    } catch (error) {
      if (error instanceof ConflictError) {
        alertError("Denne hosten er allerede konfigurert")
      } else if (error instanceof BadRequestError) {
        alertError(error.message)
      } else {
        throw error
      }
    }
  }
  
  async function handleHostDeleted(guild: Guild, host: string) {
    const updatedHostNames = [...guild.hostnames.filter(hostname => hostname != host)]
    await alertLoading("Sletter host", async () => {
      await updateGuild({... guild, hostnames: updatedHostNames })
    })
  }

  async function handleUnconfiguredGuildAdd(unconfiguredGuild: UnconfiguredGuild) {
    await alertLoading("Konfigurer", async () => {
      await configureGuild(unconfiguredGuild)
    })
  }

  return <Stack spacing={2} justifyContent="center" alignItems="center">
    {unconfiguredGuilds.length > 0 ? <>
        <Typography variant={"h5"}>Unconfigured guilds</Typography>
        <Paper sx={{ width: "100%" }}>
          <List component={"nav"}>
            {unconfiguredGuilds.map(unconfiguredGuild => <ListItemButton>
                <ListItemIcon>
                  <DiscordGuildAvatar guild={unconfiguredGuild} />
                </ListItemIcon>
                <ListItemText primary={unconfiguredGuild.name} secondary={unconfiguredGuild.id} />
                <ListItemSecondaryAction>
                  <IconButton onClick={() => handleUnconfiguredGuildAdd(unconfiguredGuild)}>
                    <Add />
                  </IconButton>
                </ListItemSecondaryAction>
              </ListItemButton>)}
          </List>
        </Paper>
      </> : <></>}
    <Typography variant={"h5"}>Configured guilds</Typography>
    <Paper sx={{ width: "100%" }}>
      {configuredGuilds.length === 0 ? <Typography sx={{ width: "100%", padding: "1em", textAlign: "center" }}>
          There are no configured guilds
        </Typography> : <List component={"nav"}>
          {configuredGuilds.map(guild => <ListItemButton
            onClick={() => handleGuildSelect(guild)}
            selected={selectedGuildId == guild.id}
            key={guild.id}
          >
            <ListItemIcon>
              <DiscordGuildAvatar guild={guild} />
            </ListItemIcon>
            <ListItemText primary={guild.name} secondary={guild.id} />
            <ListItemSecondaryAction>
              <IconButton onClick={() => navigate(`/guild/${guild.id}`)}>
                <Edit />
              </IconButton>
            </ListItemSecondaryAction>
          </ListItemButton>)}
        </List>}
    </Paper>
    {selectedGuild && <Divider orientation="horizontal" sx={{ width: "100%" }} />}
    {selectedGuild && <HostList
        guild={selectedGuild}
        onHostAdded={handleHostAdded}
        onHostDeleted={handleHostDeleted}
      />}
  </Stack>
}

function HostList(props: { guild: Guild; onHostDeleted: (guild: Guild, host: string) => Promise<void> , onHostAdded: (guild: Guild, host: string) => Promise<void> }) {
  const [ addingHost, setAddingHost ] = useState<string | undefined>()

  async function handleDeleteHostClick(host: string) {
    await props.onHostDeleted(props.guild, host)
  }
  
  async function handleSaveHostClick(host: string) {
    if (host.length > 0) {
      await props.onHostAdded(props.guild, host)
      setAddingHost(undefined)
    }
  }
  
  function handleAddHostClick() {
    setAddingHost("")
  }
  
  function handleAddHostCancelClick() {
    setAddingHost(undefined)
  }

  const renderHostEntry = (host: string) => <Stack spacing={1} width={"100%"} direction={"row"} justifyContent={"space-evenly"} key={host}>
    <TextField disabled value={host} sx={{flexGrow: 1}}></TextField>
    <IconButton onClick={() => handleDeleteHostClick(host)}>
      <Delete />
    </IconButton>
  </Stack>

  const renderAddHostTextField = (value: string) => <Stack spacing={1} width={"100%"} direction={"row"} justifyContent={"space-evenly"}>
    <TextField autoFocus onSubmit={() => handleSaveHostClick(value)} onChange={v => setAddingHost(v.target.value)} sx={{flexGrow: 1}}></TextField>
    <IconButton onClick={() => handleSaveHostClick(value)}>
      <Save color={"success"} />
    </IconButton>
    <IconButton onClick={() => handleAddHostCancelClick()}>
      <Cancel color={"error"} />
    </IconButton>
  </Stack>
  
  const renderAddHostButton= () => <Button variant="outlined" onClick={() => handleAddHostClick()}>Legg til mapping</Button>

  return <Stack spacing={2} justifyContent="center" alignItems="center" width={"100%"}>
    <Stack direction={"row"} width="100%" justifyContent="space-between" alignItems="center">
      <Typography height={"100%"} variant={"h5"}>
        Host mapping
      </Typography>
    </Stack>
    <Stack spacing={2} width={"100%"} justifyContent="center" alignItems="center">
      {props.guild.hostnames.map(host => renderHostEntry(host))}
      {typeof addingHost === "string" ? renderAddHostTextField(addingHost) : renderAddHostButton()}
    </Stack>
  </Stack>
}