import React, {useContext} from "react";
import Alert from "./Models/Alert";
import User from "./Models/User";

interface AlertContextObject {
  alert: Alert | null;
  setAlert: (user: Alert | null) => void
}

var defaultValue: AlertContextObject = { alert: null, setAlert: (alert: Alert | null) => { } };

export const AlertContext = React.createContext<AlertContextObject>(defaultValue);

export const useAlertContext = () => useContext(AlertContext)