import * as React from 'react';
import {useContext, useEffect, useState} from 'react';
import Typography from '@mui/material/Typography';
import {Link, useSearchParams} from 'react-router-dom';
import Config from "../config"
import {CircularProgress, Stack} from '@mui/material';
import {UserContext} from '../UserContext';
import Button from "@mui/material/Button";
import User from "../Adapters/Models/User";
import {CookiesAdapter} from "../Adapters/CookiesAdapter";

CookiesAdapter.setCurrentLan("6789dd19-ef5a-4f33-b830-399cb8af80f3")

export default function RedirectLogin() {
  const [user, setUser] = useState<User | null>(null)
  const userContext = useContext(UserContext)
  const [searchParams] = useSearchParams()

  useEffect(() => {
    let code = searchParams.get("code")
    if (code) {
      LoginWithDiscordToken(code, user => {
        console.log(user)
        setUser(user)
        userContext.setUser(user)
      })
    }
  }, [])

  const welcome = () => {
    return (<Stack spacing={1} justifyContent="center" alignItems="center">
      <Typography variant="h5" component="h1" gutterBottom>
        {"Velkommen, " + user?.nick}
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
