import { atom, useRecoilState } from "recoil"
import ApiRequest from "./ApiRequest"
import { ActiveGuild } from "./Models"

export const activeGuildAtom = atom<ActiveGuild | null>({
  key: "activeGuild",
  default: discoverGuild(),
})

async function discoverGuild() {
  const response = await ApiRequest("GET", `guild/discover`)
  if (response.status == 404) return null

  const activeGuild = (await response.json()) as ActiveGuild

  return activeGuild
}

export function useActiveGuildId() {
  const [activeGuild] = useRecoilState(activeGuildAtom)
  const activeGuildId = activeGuild && activeGuild.guildId

  return activeGuildId
}
