import Container from "@mui/material/Container"
import { Suspense, useState } from "react"
import { Route, Routes, useParams } from "react-router-dom"
import { useActiveGuild } from "./Adapters/ActiveGuild"
import { useGuild } from "./Adapters/Guilds/Guilds"
import { ActiveGuild } from "./Adapters/Models"
import { AlertContext, AlertModel, setupAlerts } from "./Contexts/AlertContext"
import { DialogContext, DialogModel, setupDialogs } from "./Contexts/DialogContext"
import MainAppBar from "./MainAppBar"
import AllGuildsOverview from "./Pages/AllGuildsOverview"
import ErrorPage from "./Pages/ErrorPage"
import GuildOverview from "./Pages/GuildOverview"
import { GuildRoleOverview } from "./Pages/GuildRoleOverview"
import Loading from "./Pages/Loading"
import NotFound from "./Pages/NotFound"
import RedirectLogin from "./Pages/RedirectLogin"
import Seats from "./Pages/Seats"

function GuildOverviewWrapper() {
  const params = useParams<{ guildId: string }>()
  const guild = useGuild(params.guildId as string)

  if (!guild) return <ErrorPage header="404" />

  return <GuildOverview guild={guild} />
}

function GuildRolesOverviewWrapper() {
  const params = useParams<{ guildId: string }>()
  const guild = useGuild(params.guildId as string)

  if (!guild) return <ErrorPage header="404" />

  return <GuildRoleOverview guild={guild} />
}

export default function App() {
  const activeGuild = useActiveGuild()
  const [alert, setAlert] = useState<AlertModel | null>(null)
  const [dialog, setDialog] = useState<DialogModel | null>(null)
  const { Alert, ...alertActions } = setupAlerts(setAlert)
  const { Dialog, ...dialogActions } = setupDialogs(setDialog)

  return (
    <AlertContext.Provider value={alertActions}>
      <DialogContext.Provider value={dialogActions}>
        {alert && <Alert alert={alert} />}
        {dialog && <Dialog dialog={dialog} />}
        {activeGuild ? <AppWithActiveGuild activeGuild={activeGuild} /> : <AppWithoutActiveGuild />}
      </DialogContext.Provider>
    </AlertContext.Provider>
  )
}

function AppWithoutActiveGuild() {
  return <div></div>
}

function AppWithActiveGuild(props: { activeGuild: ActiveGuild }) {
  return (
    <>
      <MainAppBar activeGuild={props.activeGuild} />
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
    </>
  )
}
