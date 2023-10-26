import React, {useContext} from "react";
import User from "./Models/User";
import Seat from "./Models/Seat";

interface AppStateContextObject {
  loggedInUser: User | null;
  seats: Seat[];
  activeLan: string;
  setLoggedInUser: (user: User | null) => void
  setSeats: (seats: Seat[]) => void
  setActiveLan: (lan: string) => void
}

var defaultValue: AppStateContextObject = {
  loggedInUser: null,
  seats: [],
  activeLan: "",
  setLoggedInUser: (user: User | null) => { },
  setSeats: (seats: Seat[]) => { },
  setActiveLan: (lan: string) => { }
};

export const AppStateContext = React.createContext<AppStateContextObject>(defaultValue);

export const useAppState = () => useContext(AppStateContext)