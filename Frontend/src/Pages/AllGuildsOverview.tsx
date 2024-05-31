/* eslint-disable @typescript-eslint/no-unused-vars */
import { Cancel } from "@mui/icons-material"
import {
  Button,
  Divider,
  IconButton,
  List,
  ListItemButton,
  Paper,
  Stack,
  TextField,
  Typography,
} from "@mui/material"
import ListItemIcon from "@mui/material/ListItemIcon"
import ListItemText from "@mui/material/ListItemText"
import { useState } from "react"
import { useGuilds } from "../Adapters/Guilds/Guilds"
import { Guild } from "../Adapters/Models"
import DiscordGuildAvatar from "../Components/DiscordAvatar"

export default function AllGuildsOverview() {
  const guilds = useGuilds()
  const [selectedGuild, setSelectedGuild] = useState<Guild | null>(null)

  function handleGuildSelect(guild: Guild) {
    setSelectedGuild(guild)
  }

  function handleHostSave(guild: Guild, hosts: string[]) {
    setSelectedGuild(guild)
  }

  return (
    <Stack spacing={2} justifyContent="center" alignItems="center">
      <Paper sx={{ width: "100%" }}>
        <List component={"nav"}>
          {guilds.map((guild) => (
            <ListItemButton
              onClick={() => handleGuildSelect(guild)}
              selected={selectedGuild?.id == guild.id}
              key={guild.id}
            >
              <ListItemIcon>
                <DiscordGuildAvatar guild={guild} />
              </ListItemIcon>
              <ListItemText primary={guild.name} secondary={guild.id} />
            </ListItemButton>
          ))}
        </List>
      </Paper>
      {selectedGuild && <Divider orientation="horizontal" sx={{ width: "100%" }} />}
      {selectedGuild && (
        <HostList
          guild={selectedGuild}
          hosts={selectedGuild}
          onHostSave={(hosts) => handleHostSave(selectedGuild, hosts)}
        />
      )}
    </Stack>
  )
}

function HostList(props: { guild: Guild; hosts: string[]; onHostSave: (hosts: string[]) => void }) {
  function handleSaveClick() {
    props.onHostSave(["asd"])
  }

  function handleDeleteHostClick(host: string) {}

  const renderHostMappingEntry = (host: string) => (
    <Stack spacing={1} width={"100%"} direction={"row"} justifyContent={"space-evenly"} key={host}>
      <TextField></TextField>
      <IconButton onClick={() => handleDeleteHostClick(host)}>
        <Cancel />
      </IconButton>
    </Stack>
  )

  return (
    <Stack spacing={2} justifyContent="center" alignItems="center" width={"100%"}>
      <Stack direction={"row"} width="100%" justifyContent="space-between" alignItems="center">
        <Typography height={"100%"} variant={"h5"}>
          Host mapping
        </Typography>
      </Stack>
      <Stack spacing={2} width={"100%"} justifyContent="center" alignItems="center">
        {props.hosts.length > 0 ? (
          props.hosts.map((mapping) => renderHostMappingEntry(mapping))
        ) : (
          <Typography>Ingen mappinger registrert</Typography>
        )}
        <Button variant="outlined">Legg til mapping</Button>
      </Stack>
    </Stack>
  )
}
