import React, {useContext} from "react";
import User from "./Models/User";

interface UserContextObject {
  user: User | null;
  setUser: (user: User | null) => void
}

var defaultValue: UserContextObject = {
  user: null, setUser: (user: User | null) => { }
};

export const UserContext = React.createContext<UserContextObject>(defaultValue);

export const useUserContext = () => useContext(UserContext)