import React, {useContext} from "react";

export interface AlertModel {
  title: string;
  description?: string;
  type: "info" | "success" | "warning" | "error" | "loading",
}

interface AlertContextObject {
  alert: AlertModel | null;
  setAlert: (alert: AlertModel | null) => void
}

var defaultValue: AlertContextObject = {
  alert: null,
  setAlert: (alert: AlertModel | null) => { }
};

export const AlertContext = React.createContext<AlertContextObject>(defaultValue);

export const useAlerts= () => useContext(AlertContext)