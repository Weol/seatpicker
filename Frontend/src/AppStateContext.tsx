import React, {useContext} from "react";
import User from "./Models/User";
import AuthenticationToken from "./Models/AuthenticationToken";
import Lan from "./Models/Lan";

export interface AppState {
  loggedInUser: User | null;
  authenticationToken: AuthenticationToken | null;
  activeLan: string;
}

interface AppStateContextObject {
  appState: AppState
  setAppState: (appState: AppState) => void
  getAuthToken: () => string | null
}

var defaultValue: AppStateContextObject = {
  appState: { loggedInUser: null, activeLan: "", authenticationToken: null },
  setAppState: (appState: AppState) => { },
  getAuthToken: () => { return null }
};

export const AppStateContext = React.createContext<AppStateContextObject>(defaultValue);

export const useAppState = () => useContext(AppStateContext)