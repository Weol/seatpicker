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
import { useState } from "react"
import { Edit } from "@mui/icons-material"
import DelayedCircularProgress from "../../Components/DelayedCircularProgress"
import ListItemText from "@mui/material/ListItemText"
import ListItemIcon from "@mui/material/ListItemIcon"
import { Guild, useGuildAdapter } from "../../Adapters/GuildAdapter"
import DiscordGuildAvatar from "../../Components/DiscordAvatar"
import { Outlet, useNavigate } from "react-router-dom"

export type AdminRouteContext = {
  guild: Guild
}

export default function Admin() {
  const navigate = useNavigate()
  const { guilds } = useGuildAdapter()
  const [selectedGuild, setSelectedGuild] = useState<Guild | null>(null)

  function handleGuildSelected(guild: Guild) {
    setSelectedGuild(guild)
    navigate(`guild/${guild.id}`)
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
        />
      ) : (
        <DelayedCircularProgress />
      )}

      {selectedGuild && (
        <Outlet
          context={{ guild: selectedGuild } satisfies AdminRouteContext}
        />
      )}
    </Stack>
  )
}

function GuildList(props: {
  guilds: Guild[]
  selectedGuild: Guild | null
  onGuildSelected: (guild: Guild) => void
}) {
  const navigate = useNavigate()

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
              <IconButton onClick={() => navigate(`roles/${guild.id}`)}>
                <Edit />
              </IconButton>
            </ListItemSecondaryAction>
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
