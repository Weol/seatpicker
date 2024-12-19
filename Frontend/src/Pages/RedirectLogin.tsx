import { CircularProgress, Stack } from "@mui/material"
import Button from "@mui/material/Button"
import Typography from "@mui/material/Typography"
import { useEffect } from "react"
import { Link, useSearchParams } from "react-router-dom"
import { useAuth } from "../Adapters/AuthAdapter"
import { ActiveGuild, User } from "../Adapters/Models"
import { DiscordUserAvatar } from "../Components/DiscordAvatar"

export default function RedirectLogin(props: { activeGuild: ActiveGuild | null }) {
  const { login, loggedInUser } = useAuth()
  const [searchParams] = useSearchParams()

  useEffect(() => {
    const code = searchParams.get("code")
    if (code && !loggedInUser) {
      login(code, props.activeGuild?.guildId ?? null)
    }
  }, [])

  const Welcome = (user: User) => {
    return (
      <Stack spacing={1} justifyContent="center" alignItems="center">
        <Typography variant="h5" component="h1" gutterBottom>
          {"Velkommen, " + user.name}
        </Typography>

        <DiscordUserAvatar
          user={user}
          style={{ width: "150px", height: "150px", borderRadius: "50%" }}
        />

        <Button component={Link} to="/" variant="contained">
          Gå til sete reservasjon
        </Button>
      </Stack>
    )
  }

  const Loading = () => {
    return (
      <Stack>
        <CircularProgress />
      </Stack>
    )
  }

  return (
    <Stack sx={{ my: 4, alignItems: "center" }}>
      {loggedInUser ? Welcome(loggedInUser) : Loading()}
    </Stack>
  )
}
