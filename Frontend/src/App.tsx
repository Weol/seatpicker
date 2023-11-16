import * as React from "react"
import { useEffect, useState } from "react"
import { Route, Routes } from "react-router-dom"
import NotFound from "./Pages/NotFound"
import Seats from "./Pages/Seats"
import RedirectLogin from "./Pages/RedirectLogin"
import Admin from "./Pages/Admin/Admin"
import MainAppBar from "./MainAppBar"
import { AlertContext, AlertModel, setupAlerts } from "./Contexts/AlertContext"
import Cookies from "universal-cookie"
import Container from "@mui/material/Container"
import {
  DialogModel,
  DialogContext,
  setupDialogs,
} from "./Contexts/DialogContext"
import { AppStateContext } from "./Contexts/AppStateContext"
import { AuthenticationToken } from "./Adapters/AuthenticationAdapter"
import { RoleMapping } from "./Pages/RoleMapping"
import Lans from "./Pages/Admin/Lans"
import { ApiContext } from "./Contexts/ApiContext"
import { Guild } from "./Adapters/GuildAdapter"
import { Lan } from "./Adapters/LanAdapter"
import { Seat } from "./Adapters/SeatsAdapter"

const cookies = new Cookies()

cookies.set("activeLan", "46d77e33-3209-4a0b-b210-5697fb6ca263")

export default function App() {
  const [authenticationToken, setAuthenticationToken] =
    useState<AuthenticationToken | null>(cookies.get("authenticationToken"))
  const [activeLan, setActiveLan] = useState<string>(cookies.get("activeLan"))
  const [guilds, setGuilds] = useState<Guild[] | null>(null)
  const [lans, setLans] = useState<Lan[] | null>(null)
  const [seats, setSeats] = useState<Seat[] | null>(null)
  const [alert, setAlert] = useState<AlertModel | null>(null)
  const [dialog, setDialog] = useState<DialogModel | null>(null)
  const { Alert, ...alertActions } = setupAlerts(setAlert)
  const { Dialog, ...dialogActions } = setupDialogs(setDialog)

  useEffect(() => {
    if (authenticationToken == null) {
      cookies.remove("authenticationToken")
    } else {
      cookies.set("authenticationToken", authenticationToken)
    }
  }, [authenticationToken])

  useEffect(() => {
    cookies.set("activeLan", activeLan)
  }, [activeLan])

  return (
    <AlertContext.Provider value={alertActions}>
      <AppStateContext.Provider
        value={{
          authenticationToken: authenticationToken,
          setAuthenticationToken: setAuthenticationToken,
          activeLan: activeLan,
          setActiveLan: setActiveLan,
        }}
      >
        <DialogContext.Provider value={dialogActions}>
          <ApiContext.Provider
            value={{
              lans,
              guilds,
              seats,
              setSeats,
              setGuilds,
              setLans,
            }}
          >
            {alert && <Alert alert={alert} />}
            {dialog && <Dialog dialog={dialog} />}
            <MainAppBar />
            <Container maxWidth="sm">
              <Routes>
                <Route path="/" element={<Seats />} />
                <Route path="/redirect-login" element={<RedirectLogin />} />
                <Route path="/admin" element={<Admin />}>
                  <Route path="roles/:guildId" element={<RoleMapping />} />
                  <Route path="guild/:guildId" element={<Lans />} />
                </Route>
                <Route path="/*" element={<NotFound />} />
              </Routes>
            </Container>
          </ApiContext.Provider>
        </DialogContext.Provider>
      </AppStateContext.Provider>
    </AlertContext.Provider>
  )
}
