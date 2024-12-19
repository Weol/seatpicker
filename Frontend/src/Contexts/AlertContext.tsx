import { Alert as MuiAlert, AlertTitle, CircularProgress, Snackbar } from "@mui/material"
import React, { useContext } from "react"

export interface AlertModel {
  title: string
  description?: string
  type: "info" | "success" | "error" | "loading" | "warning"
}

interface AlertContextObject {
  alertSuccess: (title: string, description?: string) => void
  alertError: (title: string, description?: string) => void
  alertInfo: (title: string, description?: string) => void
  alertWarning: (title: string, description?: string) => void
  alertLoading: (title: string, waitFor: () => Promise<void>) => Promise<void>
}

const defaultValue: AlertContextObject = {
  alertSuccess: () => {},
  alertError: () => {},
  alertInfo: () => {},
  alertWarning: () => {},
  alertLoading: async () => {},
}

export const AlertContext = React.createContext<AlertContextObject>(defaultValue)

export const useAlerts = () => useContext(AlertContext)

export function setupAlerts(setAlert: (alert: AlertModel | null) => void) {
  const Alert = (props: { alert: AlertModel }) => (
    <Snackbar
      anchorOrigin={{ vertical: "top", horizontal: "center" }}
      autoHideDuration={(props.alert.type != "loading" && 3000) || undefined}
      open={true}
      onClose={() => setAlert(null)}
    >
      <MuiAlert
        icon={(props.alert.type == "loading" && <CircularProgress size={"1em"} />) || undefined}
        variant="filled"
        severity={props.alert.type == "loading" ? "info" : props.alert.type}
      >
        {props.alert.description != null && <AlertTitle>{props.alert.title}</AlertTitle>}
        {props.alert.description != undefined ? props.alert.description : props.alert.title}
      </MuiAlert>
    </Snackbar>
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

  return {
    Alert,
    alertInfo,
    alertError,
    alertLoading,
    alertWarning,
    alertSuccess,
  }
}
