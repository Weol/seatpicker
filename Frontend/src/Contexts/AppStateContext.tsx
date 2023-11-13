import React, { useContext } from "react"
import { AuthenticationToken } from "../Adapters/AuthenticationAdapter"

interface AppStateContextObject {
  authenticationToken: AuthenticationToken | null
  activeLan: string
  setActiveLan: (lanId: string) => void
  setAuthenticationToken: (
    authenticationToken: AuthenticationToken | null
  ) => void
}

const defaultValue: AppStateContextObject = {
  authenticationToken: null,
  activeLan: "",
  setActiveLan: () => {},
  setAuthenticationToken: () => {},
}

export const AppStateContext =
  React.createContext<AppStateContextObject>(defaultValue)

export const useAppState = () => useContext(AppStateContext)
