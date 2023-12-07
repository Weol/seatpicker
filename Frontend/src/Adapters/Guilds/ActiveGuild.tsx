import { atom, useRecoilState } from "recoil"
import { Role } from "../AuthAdapter"
import { synchronizeWithLocalStorage } from "../Utils"

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

export const activeGuildIdAtom = atom<string>({
  key: "activeGuildId",
  default: "654016371260260412",
  effects: [synchronizeWithLocalStorage("activeGuildId")],
})

export function useActiveGuildId() {
  const [activeGuildId, setActiveGuildId] = useRecoilState(activeGuildIdAtom)

  return { activeGuildId, setActiveGuildId }
}
