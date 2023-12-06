import Container from "@mui/material/Container"
import { useState } from "react"
import { ErrorBoundary } from "react-error-boundary"
import { Route, Routes, useParams } from "react-router-dom"
import { AlertContext, AlertModel, setupAlerts } from "./Contexts/AlertContext"
import { DialogContext, DialogModel, setupDialogs } from "./Contexts/DialogContext"
import MainAppBar from "./MainAppBar"
import Admin from "./Pages/Admin"
import ErrorPage from "./Pages/ErrorPage"
import GuildSettings from "./Pages/GuildSettings"
import NotFound from "./Pages/NotFound"
import RedirectLogin from "./Pages/RedirectLogin"
import Seats from "./Pages/Seats"

function GuildSettingsWrapper() {
  const params = useParams<{ guildId: string }>()

  return <GuildSettings guildId={params.guildId as string} />
}

export function GuildSettingsPath(guildId: string) {
  return `/admin/guild/${guildId}`
}

export default function App() {
  const [alert, setAlert] = useState<AlertModel | null>(null)
  const [dialog, setDialog] = useState<DialogModel | null>(null)
  const { Alert, ...alertActions } = setupAlerts(setAlert)
  const { Dialog, ...dialogActions } = setupDialogs(setDialog)

  return (
    <AlertContext.Provider value={alertActions}>
      <DialogContext.Provider value={dialogActions}>
        {alert && <Alert alert={alert} />}
        {dialog && <Dialog dialog={dialog} />}
        <ErrorBoundary fallback={<ErrorPage />}>
          <MainAppBar />
          <Container maxWidth="sm" sx={{ paddingTop: "1em" }}>
            <Routes>
              <Route path="/" element={<Seats />} />
              <Route path="/redirect-login" element={<RedirectLogin />} />
              <Route path="/admin" element={<Admin />} />
              <Route path="/admin/guild/:guildId" element={<GuildSettingsWrapper />} />
              <Route path="/*" element={<NotFound />} />
            </Routes>
          </Container>
        </ErrorBoundary>
      </DialogContext.Provider>
    </AlertContext.Provider>
  )
}
