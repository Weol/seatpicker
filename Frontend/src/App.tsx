import Container from "@mui/material/Container"
import { Suspense, useState } from "react"
import { Route, Routes, useParams } from "react-router-dom"
import { AlertContext, AlertModel, setupAlerts } from "./Contexts/AlertContext"
import { DialogContext, DialogModel, setupDialogs } from "./Contexts/DialogContext"
import MainAppBar from "./MainAppBar"
import AllGuildsOverview from "./Pages/AllGuildsOverview"
import Loading from "./Pages/Loading"
import NotFound from "./Pages/NotFound"
import RedirectLogin from "./Pages/RedirectLogin"
import Seats from "./Pages/Seats"
import GuildOverview from "./Pages/GuildOverview"
import GuildRoleOverview from "./Pages/GuildRoleOverview"

function GuildOverviewWrapper() {
  const params = useParams<{ guildId: string }>()

  return <GuildOverview guildId={params.guildId as string} />
}

function GuildRolesOverviewWrapper() {
  const params = useParams<{ guildId: string }>()

  return <GuildRoleOverview guildId={params.guildId as string} />
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
        <MainAppBar />
        <Suspense fallback={<Loading />}>
          <Container maxWidth="sm" sx={{ paddingTop: "1em" }}>
            <Routes>
              <Route path="/" element={<Seats />} />
              <Route path="/redirect-login" element={<RedirectLogin />} />
              <Route path="/guilds" element={<AllGuildsOverview />} />
              <Route path="/guild/:guildId" element={<GuildOverviewWrapper />} />
              <Route path="/guild/:guildId/roles" element={<GuildRolesOverviewWrapper />} />
              <Route path="*" element={<NotFound />} />
            </Routes>
          </Container>
        </Suspense>
      </DialogContext.Provider>
    </AlertContext.Provider>
  )
}
