import Container from "@mui/material/Container"
import { ReactNode, Suspense, useState } from "react"
import { Route, Routes, useParams } from "react-router-dom"
import { useActiveGuildId } from "./Adapters/ActiveGuild"
import { AlertContext, AlertModel, setupAlerts } from "./Contexts/AlertContext"
import { DialogContext, DialogModel, setupDialogs } from "./Contexts/DialogContext"
import MainAppBar from "./MainAppBar"
import AllGuildsOverview from "./Pages/AllGuildsOverview"
import ErrorPage from "./Pages/ErrorPage"
import GuildOverview from "./Pages/GuildOverview"
import GuildRoleOverview from "./Pages/GuildRoleOverview"
import Loading from "./Pages/Loading"
import RedirectLogin from "./Pages/RedirectLogin"
import Seats from "./Pages/Seats"

/**
 * Wrappers
 */

function GuildOverviewWrapper() {
  const params = useParams<{ guildId: string }>()

  return <GuildOverview guildId={params.guildId as string} />
}

function GuildRolesOverviewWrapper() {
  const params = useParams<{ guildId: string }>()

  return <GuildRoleOverview guildId={params.guildId as string} />
}

function ActiveGuildWrapper(props: { children: ReactNode }) {
  const activeGuildId = useActiveGuildId()

  return activeGuildId != null ? <>{props.children}</> : <ActiveGuildIdNotFound />
}

/**
 * Error pages
 */
function PageNotFound() {
  return <ErrorPage header={"404"} subtitle="Fant ikke hva enn du leter etter" />
}

function ActiveGuildIdNotFound() {
  return <ErrorPage header={"404"} subtitle="No active guild id is configured for this host" />
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
              <Route
                path="/"
                element={
                  <ActiveGuildWrapper>
                    <Seats />
                  </ActiveGuildWrapper>
                }
              />
              <Route path="/redirect-login" element={<RedirectLogin />} />
              <Route path="/guilds" element={<AllGuildsOverview />} />
              <Route path="/guild/:guildId" element={<GuildOverviewWrapper />} />
              <Route path="/guild/:guildId/roles" element={<GuildRolesOverviewWrapper />} />
              <Route path="*" element={<PageNotFound />} />
            </Routes>
          </Container>
        </Suspense>
      </DialogContext.Provider>
    </AlertContext.Provider>
  )
}
