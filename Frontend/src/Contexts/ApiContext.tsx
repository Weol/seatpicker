/* eslint-disable @typescript-eslint/no-unused-vars */
import React, { useContext } from "react"
import { Seat } from "../Adapters/SeatsAdapter"
import { Guild } from "../Adapters/GuildAdapter"
import { Lan } from "../Adapters/LanAdapter"

interface ApiContextObject {
  guilds: Guild[] | null
  lans: Lan[] | null
  seats: Seat[] | null
  setGuilds: (guilds: Guild[]) => void
  setLans: (lans: Lan[]) => void
  setSeats: (seats: Seat[]) => void
}

const defaultValue: ApiContextObject = {
  guilds: null,
  lans: null,
  seats: null,
  setGuilds: (guilds: Guild[]) => {},
  setLans: (lans: Lan[]) => {},
  setSeats: (seats: Seat[]) => {},
}

export const ApiContext = React.createContext<ApiContextObject>(defaultValue)

export const useApi = () => useContext(ApiContext)
export const useLans = (initializer: () => Promise<Lan[]>) => {
  // Lets not do this, lets do it inn App.tsx

  return useContext(ApiContext)
}
