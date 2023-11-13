import * as React from "react"
import { useEffect, useState } from "react"
import { Route, Routes } from "react-router-dom"
import NotFound from "./Pages/NotFound"
import Seats from "./Pages/Seats"
import RedirectLogin from "./Pages/RedirectLogin"
import Admin from "./Pages/Admin/Admin"
import MainAppBar from "./MainAppBar"
import { AlertContext, AlertModel } from "./Contexts/AlertContext"
import Cookies from "universal-cookie"
import Container from "@mui/material/Container"
import {
  Alert,
  AlertTitle,
  CircularProgress,
  DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle,
  Snackbar,
  Dialog as MuiDialog,
} from "@mui/material"
import {
  DialogModel,
  DialogContext,
  DialogResponse,
} from "./Contexts/DialogContext"
import Button from "@mui/material/Button"
import { AppStateContext } from "./Contexts/AppStateContext"
import { AuthenticationToken } from "./Adapters/AuthenticationAdapter"

const cookies = new Cookies()

cookies.set("activeLan", "46d77e33-3209-4a0b-b210-5697fb6ca263")

export default function App() {
  const [authenticationToken, setAuthenticationToken] =
    useState<AuthenticationToken | null>(cookies.get("authenticationToken"))
  const [activeLan, setActiveLan] = useState<string>(cookies.get("activeLan"))
  const [alert, setAlert] = useState<AlertModel | null>(null)
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const [dialog, setDialog] = useState<DialogModel<any> | null>(null)

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

  function showDialog<T>(
    title: string,
    description: string,
    metadata: T,
    positiveText: string,
    negativeText: string
  ): Promise<DialogResponse<T>> {
    return new Promise<DialogResponse<T>>((resolve, reject) => {
      setDialog({
        title: title,
        description: description,
        metadata: metadata,
        positiveText: positiveText,
        negativeText: negativeText,
        resolve: resolve,
        reject: reject,
      })
    })
  }

  const renderWithDescription = (alert: AlertModel) => (
    <Alert
      icon={
        (alert.type == "loading" && <CircularProgress size={"1em"} />) ||
        undefined
      }
      variant="filled"
      severity={alert.type == "loading" ? "info" : alert.type}
    >
      <AlertTitle>{alert.title}</AlertTitle>
      {alert.description}
    </Alert>
  )

  const renderWithoutDescription = (alert: AlertModel) => (
    <Alert
      icon={
        (alert.type == "loading" && <CircularProgress size={"1em"} />) ||
        undefined
      }
      variant="filled"
      severity={alert.type == "loading" ? "info" : alert.type}
    >
      {alert.title}
    </Alert>
  )

  const renderAlert = (alert: AlertModel) => (
    <Snackbar
      anchorOrigin={{ vertical: "top", horizontal: "center" }}
      autoHideDuration={(alert.type != "loading" && 3000) || undefined}
      open={true}
      onClose={() => setAlert(null)}
    >
      {(alert.description == undefined && renderWithoutDescription(alert)) ||
        renderWithDescription(alert)}
    </Snackbar>
  )

  const renderDialog = () =>
    dialog && (
      <MuiDialog
        open={true}
        onClose={() => setDialog(null)}
        aria-labelledby="alert-dialog-title"
        aria-describedby="alert-dialog-description"
      >
        <DialogTitle id="alert-dialog-title">{dialog.title}</DialogTitle>
        <DialogContent>
          <DialogContentText id="alert-dialog-description">
            {dialog.description}
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button
            onClick={() => {
              dialog.resolve({
                positive: true,
                metadata: dialog.metadata,
              })

              setDialog(null)
            }}
          >
            {dialog.negativeText}
          </Button>
          <Button
            onClick={() => {
              dialog.resolve({
                positive: false,
                metadata: dialog.metadata,
              })

              setDialog(null)
            }}
          >
            {dialog.positiveText}
          </Button>
        </DialogActions>
      </MuiDialog>
    )

  const alertSuccess = (title: string, description?: string) => {
    setAlert({
      type: "success",
      title: title,
      description: description,
    })
  }

  const alertError = (title: string, description?: string) => {
    setAlert({
      type: "error",
      title: title,
      description: description,
    })
  }

  const alertInfo = (title: string, description?: string) => {
    setAlert({
      type: "info",
      title: title,
      description: description,
    })
  }

  const alertLoading = async (title: string, waitFor: () => Promise<void>) => {
    const id = setTimeout(() => {
      setAlert({
        type: "loading",
        title: title,
      })
    }, 1000)

    try {
      await waitFor()
    } catch (e) {
      clearTimeout(id)
      throw e
    }

    clearTimeout(id)
  }

  const alertWarning = (title: string, description?: string) => {
    setAlert({
      type: "warning",
      title: title,
      description: description,
    })
  }

  return (
    <AlertContext.Provider
      value={{
        alertInfo: alertInfo,
        alertLoading: alertLoading,
        alertError: alertError,
        alertSuccess: alertSuccess,
        alertWarning: alertWarning,
      }}
    >
      <AppStateContext.Provider
        value={{
          authenticationToken: authenticationToken,
          setAuthenticationToken: setAuthenticationToken,
          activeLan: activeLan,
          setActiveLan: setActiveLan,
        }}
      >
        <DialogContext.Provider value={{ showDialog: showDialog }}>
          {alert && renderAlert(alert)}
          {dialog && renderDialog()}
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
      </AppStateContext.Provider>
    </AlertContext.Provider>
  )
}
