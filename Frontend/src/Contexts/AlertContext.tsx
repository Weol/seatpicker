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

export const AlertContext =
  React.createContext<AlertContextObject>(defaultValue)

export const useAlerts = () => useContext(AlertContext)
