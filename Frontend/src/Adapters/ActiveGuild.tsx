import {atom, useRecoilValue} from "recoil"
import ApiRequest from "./ApiRequest"
import {ActiveGuild} from "./Models"

async function discoverGuild() {
  const response = await ApiRequest("GET", `guild/discover`)
  if (response.status != 200) return null

  const activeGuild = (await response.json()) as ActiveGuild

  return activeGuild
}

export const activeGuildAtom = atom<ActiveGuild | null>({
  key: "activeGuild",
  default: discoverGuild(),
})

export function useActiveGuild() {
  return useRecoilValue(activeGuildAtom)
}
