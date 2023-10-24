import * as React from 'react';
import {useState} from 'react';
import {Route, Routes} from "react-router-dom";
import NotFound from './Pages/NotFound';
import Seats from './Pages/Seats';
import RedirectLogin from './Pages/RedirectLogin';
import MainAppBar from './MainAppBar';
import {AlertContext} from './AlertContext';
import AlertModel from "./Models/Alert";
import {Alert, AlertTitle, Snackbar} from "@mui/material";
import User from "./Models/User";
import {AuthenticationAdapter} from "./Adapters/AuthenticationAdapter";
import {UserContext} from "./UserContext";

export default function App() {
  let [user, setUser] = useState<User | null>(AuthenticationAdapter.getUser())
  let [alert, setAlert] = useState<AlertModel | null>(null)

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
      <UserContext.Provider value={{user: user, setUser: setUser}}>
        {alert && renderAlert(alert)}
        <MainAppBar/>
        <Routes>
          <Route path="/" element={<Seats/>}/>
          <Route path="/redirect-login" element={<RedirectLogin/>}/>
          <Route path="/*" element={<NotFound/>}/>
        </Routes>
      </UserContext.Provider>
    </AlertContext.Provider>
  );
}
