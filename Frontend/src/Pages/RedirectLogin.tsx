import * as React from 'react';
import {useContext, useEffect, useState} from 'react';
import Typography from '@mui/material/Typography';
import {Link, useSearchParams} from 'react-router-dom';
import Config from "../config"
import {CircularProgress, Stack} from '@mui/material';
import Button from "@mui/material/Button";
import useAuthentication from "../AuthenticationHook";
import useLoggedInUser from "../LoggedInUserHook";

export default function RedirectLogin() {
  const { login } = useAuthentication()
  const user = useLoggedInUser()
  const [searchParams] = useSearchParams()

  useEffect(() => {
    let code = searchParams.get("code")
    if (code) {
      login(code)
    }
  }, [])

  const welcome = () => {
    return (<Stack spacing={1} justifyContent="center" alignItems="center">
      <Typography variant="h5" component="h1" gutterBottom>
        {"Velkommen, " + user?.name}
      </Typography>

      <img src={Config.DiscordAvatarBaseUrl + user?.id + "/" + user?.avatar} style={{maxWidth: '150px', borderRadius: '50%'}}/>

      <Button component={Link} to="/" variant="contained">
        Gå til sete reservasjon
      </Button>
    </Stack>)
  }

  const loading = () => {
    return (<Stack>
      <CircularProgress/>
    </Stack>)
  }

  return (
    <Stack sx={{my: 4, alignItems: 'center'}}>
      {user ? welcome() : loading()}
    </Stack>
  )
}
