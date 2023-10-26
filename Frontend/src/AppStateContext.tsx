import React, {useContext} from "react";
import User from "./Models/User";
import Cookies from 'universal-cookie';
import AuthenticationToken from "./Models/AuthenticationToken";

const cookies = new Cookies();

export interface AppState {
  loggedInUser: User | null;
  authenticationToken: AuthenticationToken | null;
  activeLan: string;
}

interface AppStateContextObject {
  appState: AppState
  setAppState: (appState: AppState) => void
}

var defaultValue: AppStateContextObject = {
  appState: { loggedInUser: null, activeLan: "", authenticationToken: null },
  setAppState: (appState: AppState) => { }
};

export const AppStateContext = React.createContext<AppStateContextObject>(defaultValue);

export const useAppState = () => useContext(AppStateContext)