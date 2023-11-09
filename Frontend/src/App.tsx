import * as React from 'react';
import {useEffect, useState} from 'react';
import {Route, Routes} from "react-router-dom";
import NotFound from './Pages/NotFound';
import Seats from './Pages/Seats';
import RedirectLogin from './Pages/RedirectLogin';
import Admin from './Pages/Admin';
import MainAppBar from './MainAppBar';
import {AlertContext, AlertModel} from './AlertContext';
import {AppState, AppStateContext} from './AppStateContext';
import Cookies from 'universal-cookie';
import Container from "@mui/material/Container";
import {
  Alert,
  AlertTitle,
  CircularProgress, DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle,
  LinearProgress,
  Snackbar,
  Dialog as MuiDialog
} from "@mui/material";
import {Dialog, DialogContext} from "./DialogContext"
import Button from "@mui/material/Button";

const cookies = new Cookies();

cookies.set("activeLan", "6789dd19-ef5a-4f33-b830-399cb8af80f3")

export default function App() {
  let [appState, setAppState] = useState<AppState>({
    activeLan: cookies.get("activeLan"),
    loggedInUser: cookies.get("loggedInUser"),
    authenticationToken: cookies.get("authenticationToken")
  })
  let [alert, setAlert] = useState<AlertModel | null>(null)
  let [dialog, setDialog] = useState<Dialog<any> | null>(null)

  useEffect(() => {
    cookies.set("activeLan", appState.activeLan)

    if (appState.loggedInUser == null) {
      cookies.remove("loggedInUser")
    } else {
      cookies.set("loggedInUser", appState.loggedInUser)
    }

    if (appState.authenticationToken == null) {
      cookies.remove("authenticationToken")
    } else {
      cookies.set("authenticationToken", appState.authenticationToken)
    }
  }, [appState])

  const showDialog = (dialog: Dialog<any>) => {
    setDialog(dialog)
  }

  const getAuthToken = (): string | null => {
    return (appState.authenticationToken != null) ? appState.authenticationToken.token : null
  }

  const renderWithDescription = (alert: AlertModel) => (
    <Alert icon={alert.type == "loading" && <CircularProgress size={"1em"}/> || undefined} variant="filled"
           severity={alert.type == "loading" ? "info" : alert.type}>
      <AlertTitle>{alert.title}</AlertTitle>
      {alert.description}
    </Alert>
  )

  const renderWithoutDescription = (alert: AlertModel) => (
    <Alert icon={alert.type == "loading" && <CircularProgress size={"1em"}/> || undefined} variant="filled"
           severity={alert.type == "loading" ? "info" : alert.type}>
      {alert.title}
    </Alert>
  )

  const renderAlert = (alert: AlertModel) => (
    <Snackbar anchorOrigin={{vertical: "top", horizontal: "center"}}
              autoHideDuration={alert.type != "loading" && 3000 || undefined} open={true}
              onClose={() => setAlert(null)}>
      {alert.description == undefined && renderWithoutDescription(alert) || renderWithDescription(alert)}
    </Snackbar>
  )
  
  const renderDialog = () =>
    (dialog &&
        <MuiDialog
            open={true}
            onClose={() => setDialog(null)}
            aria-labelledby="alert-dialog-title"
            aria-describedby="alert-dialog-description"
        >
            <DialogTitle id="alert-dialog-title">
              {dialog.title}
            </DialogTitle>
            <DialogContent>
                <DialogContentText id="alert-dialog-description">
                  {dialog.description}
                </DialogContentText>
            </DialogContent>
            <DialogActions>
                <Button onClick={() => {
                  setDialog(null)
                  if (dialog && dialog.negativeCallback != undefined) dialog.negativeCallback(dialog.metadata);
                }}>{dialog.negativeText}</Button>
                <Button onClick={() => {
                  setDialog(null)
                  if (dialog && dialog.positiveCallback != undefined) dialog.positiveCallback(dialog.metadata);
                }}>{dialog.positiveText}</Button>
            </DialogActions>
        </MuiDialog>
    )

  const alertSuccess = (title: string, description?: string) => {
    setAlert({
      type: "success",
      title: title,
      description: description
    })
  }

  const alertError = (title: string, description?: string) => {
    setAlert({
      type: "error",
      title: title,
      description: description
    })
  }

  const alertInfo = (title: string, description?: string) => {
    setAlert({
      type: "info",
      title: title,
      description: description
    })
  }

  const alertLoading = async (title: string, waitFor: () => Promise<void>) => {
    let id = setTimeout(() => {
      setAlert({
        type: "loading",
        title: title
      })
    }, 1000);

    try {
      await waitFor();
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
      description: description
    })
  }

  return (
    <AlertContext.Provider value={{
      alertInfo: alertInfo,
      alertLoading: alertLoading,
      alertError: alertError,
      alertSuccess: alertSuccess,
      alertWarning: alertWarning
    }}>
      <AppStateContext.Provider value={{appState, setAppState, getAuthToken}}>
        <DialogContext.Provider value={{showDialog: showDialog}}>
          {alert && renderAlert(alert)}
          {dialog && renderDialog()}
          <MainAppBar/>
          <Container maxWidth="sm">
            <Routes>
              <Route path="/" element={<Seats/>}/>
              <Route path="/redirect-login" element={<RedirectLogin/>}/>
              <Route path="/admin" element={<Admin/>}/>
              <Route path="/*" element={<NotFound/>}/>
            </Routes>
          </Container>
        </DialogContext.Provider>
      </AppStateContext.Provider>
    </AlertContext.Provider>
  );
}
