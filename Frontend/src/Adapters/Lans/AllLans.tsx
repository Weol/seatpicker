import { atomFamily, selector, useRecoilState, useRecoilValue } from "recoil"
import { activeGuildAtom } from "../ActiveGuild"
import { ApiRequest } from "../ApiRequest"
import { Lan } from "../Models"

function getLanUrl(guildId: string, lanId?: string | null) {
  return lanId == null ? `guild/${guildId}/lan/` : `guild/${guildId}/lan/${lanId}`
}

async function loadAllLans(guildId: string) {
  const response = await ApiRequest("GET", getLanUrl(guildId))

  const lans = await response.json() as Lan[]
  lans.forEach(lan => {
    lan.createdAt = new Date(lan.createdAt)
    lan.updatedAt = new Date(lan.updatedAt)
  })

  return lans
}

export const allLansAtomFamily = atomFamily<Lan[], string>({
  key: "allLans",
  default: loadAllLans,
})

export const activeLanSelector = selector<Lan | null>({
  key: "activeLan",
  get: async ({ get }) => {
    const activeGuild = get(activeGuildAtom)

    if (activeGuild == null || activeGuild.lan == null) return null
    return activeGuild.lan
  },
})

export function useActiveLan() {
  return useRecoilValue(activeLanSelector)
}

export function useAllLans(guildId: string) {
  if (!guildId) throw new Error("Guild id cannot be null")
  const [lans, setLans] = useRecoilState(allLansAtomFamily(guildId))

  const createLan = async (title: string, background: string) => {
    await ApiRequest("POST", getLanUrl(guildId), {
      guildId,
      title,
      background,
    })
    setLans(await loadAllLans(guildId))
  }

  const updateLan = async (lan: Lan, active: boolean, title: string, background: string) => {
    await ApiRequest("PUT", getLanUrl(guildId) + `${lan.id}`, {
      id: lan.id,
      active: active,
      title,
      background,
    })
    setLans(await loadAllLans(guildId))
  }

  const deleteLan = async (lan: Lan) => {
    await ApiRequest("DELETE", getLanUrl(guildId, lan.id))
    setLans(await loadAllLans(guildId))
  }

  return {
    lans,
    createLan,
    deleteLan,
    updateLan
  }
}
