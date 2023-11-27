import {
  IconButton,
  List,
  ListItemButton,
  ListItemSecondaryAction,
  Paper,
  Stack,
} from "@mui/material"
import Typography from "@mui/material/Typography"
import Divider from "@mui/material/Divider"
import { Edit } from "@mui/icons-material"
import DelayedCircularProgress from "../../Components/DelayedCircularProgress"
import ListItemText from "@mui/material/ListItemText"
import ListItemIcon from "@mui/material/ListItemIcon"
import { Guild, useGuilds } from "../../Adapters/GuildAdapter"
import DiscordGuildAvatar from "../../Components/DiscordAvatar"
import LanOverview from "./LanOverview"
import { useState } from "react"
import { RoleMappingModal } from "./RoleMappingModal"

function GetFirstGuildOrNull(guilds: Guild[] | null) {
  return guilds && guilds.length > 0 ? guilds[0] : null
}

export default function Admin() {
  const guilds = useGuilds()
  const [selectedGuild, setSelectedGuild] = useState<Guild | null>(
    GetFirstGuildOrNull(guilds)
  )
  const [editingGuild, setEditingGuild] = useState<Guild | null>(null)

  function handleGuildSelected(guild: Guild) {
    setSelectedGuild(guild)
  }

  function handleGuildEditPressed(guild: Guild) {
    setEditingGuild(guild)
  }

  function handleRoleMappingModalClose(): void {
    setEditingGuild(null)
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
      {guilds ? (
        <GuildList
          guilds={guilds}
          selectedGuild={selectedGuild}
          onGuildSelected={handleGuildSelected}
          onGuildEditPressed={handleGuildEditPressed}
        />
      ) : (
        <DelayedCircularProgress />
      )}

      {selectedGuild && <LanOverview guild={selectedGuild} />}
      {selectedGuild && (
        <RoleMappingModal
          guild={selectedGuild}
          open={editingGuild != null}
          onClose={handleRoleMappingModalClose}
        />
      )}
    </Stack>
  )
}

function GuildList(props: {
  guilds: Guild[]
  selectedGuild: Guild | null
  onGuildSelected: (guild: Guild) => void
  onGuildEditPressed: (guild: Guild) => void
}) {
  return (
    <Paper sx={{ width: "100%" }}>
      <List component={"nav"}>
        {props.guilds.map((guild) => (
          <ListItemButton
            onClick={() => props.onGuildSelected(guild)}
            selected={props.selectedGuild?.id == guild.id}
            key={guild.id}
          >
            <ListItemSecondaryAction>
              <IconButton onClick={() => props.onGuildEditPressed(guild)}>
                <Edit />
              </IconButton>
            </ListItemSecondaryAction>
            <ListItemIcon>
              <DiscordGuildAvatar guild={guild} />
            </ListItemIcon>
            <ListItemText primary={guild.name} secondary={"Id: " + guild.id} />
          </ListItemButton>
        ))}
      </List>
    </Paper>
  )
}
