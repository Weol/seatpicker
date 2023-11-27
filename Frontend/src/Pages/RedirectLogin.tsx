import * as React from "react"
import Typography from "@mui/material/Typography"
import { Link, useSearchParams } from "react-router-dom"
import { CircularProgress, Stack } from "@mui/material"
import Button from "@mui/material/Button"
import {
  User,
  useAuthenticationAdapter,
} from "../Adapters/AuthenticationAdapter"
import { useEffect } from "react"
import { DiscordUserAvatar } from "../Components/DiscordAvatar"

export default function RedirectLogin() {
  const { login, loggedInUser } = useAuthenticationAdapter()
  const [searchParams] = useSearchParams()

  useEffect(() => {
    const code = searchParams.get("code")
    if (code) {
      login(code)
    }
  }, [])

  const welcome = (user: User) => {
    return (
      <Stack spacing={1} justifyContent="center" alignItems="center">
        <Typography variant="h5" component="h1" gutterBottom>
          {"Velkommen, " + user.name}
        </Typography>

        <DiscordUserAvatar
          user={user}
          style={{ maxWidth: "150px", borderRadius: "50%" }}
        />

        <Button component={Link} to="/" variant="contained">
          Gå til sete reservasjon
        </Button>
      </Stack>
    )
  }

  const loading = () => {
    return (
      <Stack>
        <CircularProgress />
      </Stack>
    )
  }

  return (
    <Stack sx={{ my: 4, alignItems: "center" }}>
      {loggedInUser ? welcome(loggedInUser) : loading()}
    </Stack>
  )
}
