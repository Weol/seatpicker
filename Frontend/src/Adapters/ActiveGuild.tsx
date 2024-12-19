import { atom, useRecoilValue } from "recoil"
import { ApiRequest } from "./ApiRequest"
import { ActiveGuild } from "./Models"
import { NotFoundError } from "./AdapterError"

async function discoverGuild() {
  try {
    const response = await ApiRequest("GET", `guild/discover`)
    const activeGuild = (await response.json()) as ActiveGuild
    if (activeGuild.lan) {
      activeGuild.lan.createdAt = new Date(activeGuild.lan.createdAt)
      activeGuild.lan.updatedAt = new Date(activeGuild.lan.updatedAt)
    }

    return activeGuild
  } catch (response) {
    if (response instanceof NotFoundError) return null
    throw response
  }
}

export const activeGuildAtom = atom<ActiveGuild | null>({
  key: "activeGuild",
  default: discoverGuild(),
})

export function useActiveGuild() {
  return useRecoilValue(activeGuildAtom)
}
