import { useState } from "react"
import { Route, Routes } from "react-router-dom"
import NotFound from "./Pages/NotFound"
import Seats from "./Pages/Seats"
import RedirectLogin from "./Pages/RedirectLogin"
import Admin from "./Pages/Admin"
import MainAppBar from "./MainAppBar"
import { AlertContext, AlertModel, setupAlerts } from "./Contexts/AlertContext"
import Container from "@mui/material/Container"
import {
  DialogModel,
  DialogContext,
  setupDialogs,
} from "./Contexts/DialogContext"

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
        <Container maxWidth="sm">
          <Routes>
            <Route path="/" element={<Seats />} />
            <Route path="/redirect-login" element={<RedirectLogin />} />
            <Route path="/admin" element={<Admin />} />
            <Route path="/*" element={<NotFound />} />
          </Routes>
        </Container>
      </DialogContext.Provider>
    </AlertContext.Provider>
  )
}
