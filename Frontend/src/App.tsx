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

cookies.set("activeLan", "6789dd19-ef5a-4f33-b830-399cb8af80f3")

export default function App() {
  let [appState, setAppState] = useState<AppState>({ activeLan: cookies.get("activeLan"), loggedInUser: cookies.get("loggedInUser"), authenticationToken: null })
  let [alert, setAlert] = useState<AlertModel | null>(null)

  useEffect(() => {
    cookies.set("activeLan", appState.activeLan)
    if (appState.loggedInUser == null) {
      cookies.remove("loggedInUser")
    } else {
      cookies.set("loggedInUser", appState.loggedInUser)
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
