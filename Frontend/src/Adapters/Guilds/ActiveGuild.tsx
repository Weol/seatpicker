import { atom, useRecoilState } from "recoil"
import ApiRequest from "../ApiRequest"
import { Role } from "../AuthAdapter"

export interface DiscoveredGuild {
  guildId: string
}

export interface Guild {
  id: string
  name: string
  icon: string | null
}

export interface GuildRole {
  id: string
  name: string
  color: number
  roles: Role[]
}

export const activeGuildIdAtom = atom<string | null>({
  key: "activeGuildId",
  effects: [({ setSelf }) => setSelf(DiscoverGuild())],
})

async function DiscoverGuild() {
  const response = await ApiRequest("GET", `guild/discover`)
  if (response.status == 404) return null

  const discoveredGuild = (await response.json()) as DiscoveredGuild

  return discoveredGuild.guildId
}

export function useActiveGuildId() {
  const [activeGuildId] = useRecoilState(activeGuildIdAtom)

  return { activeGuildId }
}
