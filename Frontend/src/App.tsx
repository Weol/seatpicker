import * as React from 'react';
import {createContext, useEffect, useState} from 'react';
import {Route, Routes} from "react-router-dom";
import NotFound from './Pages/NotFound';
import Seats from './Pages/Seats';
import RedirectLogin from './Pages/RedirectLogin';
import LanManagement from './Pages/LanManagement';
import MainAppBar from './MainAppBar';
import {AlertContext} from './AlertContext';
import AlertModel from "./Models/Alert";
import {Alert, AlertTitle, Snackbar} from "@mui/material";
import {AppState, AppStateContext} from './AppStateContext';
import Cookies from 'universal-cookie';

const cookies = new Cookies();

cookies.set("activeLan", "3af0cfba-305e-46c6-b44e-0f70f2b6585c")

export default function App() {
  let [appState, setAppState] = useState<AppState>({ activeLan: cookies.get("activeLan"), loggedInUser: cookies.get("loggedInUser"), authenticationToken: cookies.get("authenticationToken") })
  let [alert, setAlert] = useState<AlertModel | null>(null)

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

  const renderAlert = (alert: AlertModel) => (
    <Snackbar anchorOrigin={{vertical: "top", horizontal: "center"}} autoHideDuration={3000} open={true}
              onClose={() => setAlert(null)}>
      <Alert severity={alert.type}>
        <AlertTitle>{alert.title}</AlertTitle>
        {alert.description}
      </Alert>
    </Snackbar>
  )

  return (
    <AlertContext.Provider value={{alert: alert, setAlert: setAlert}}>
      <AppStateContext.Provider value={{appState, setAppState}}>
        {alert && renderAlert(alert)}
        <MainAppBar/>
        <Routes>
          <Route path="/" element={<Seats/>}/>
          <Route path="/redirect-login" element={<RedirectLogin/>}/>
          <Route path="/lanmanagement" element={<LanManagement/>}/>
          <Route path="/*" element={<NotFound/>}/>
        </Routes>
      </AppStateContext.Provider>
    </AlertContext.Provider>
  );
}
