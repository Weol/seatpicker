import { selector, useRecoilValue } from "recoil"
import ApiRequest from "../ApiRequest"
import { activeGuildIdAtom } from "../Guilds/ActiveGuild"
import { Lan } from "./Lans"

export const activeLanSelector = selector<Lan | null>({
  key: "activeLan",
  get: async ({ get }) => {
    const activeGuildId = get(activeGuildIdAtom)
    const response = await ApiRequest("GET", `lan/active?guildId=${activeGuildId}`)

    if (response.status == 404) return null

    const lan = (await response.json()) as Lan

    lan.createdAt = new Date(lan.createdAt)
    lan.updatedAt = new Date(lan.updatedAt)

    return lan
  },
})

export function useActiveLan() {
  const activeLan = useRecoilValue(activeLanSelector)

  return activeLan
}
