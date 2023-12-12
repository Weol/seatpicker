import { atom, useRecoilState } from "recoil"
import { Role } from "../AuthAdapter"
import { synchronizeWithLocalStorage } from "../Utils"
import Config from "../../config"

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

const defaultGuildId = Config.IsLocalhost ? "817425364656586762" : "817425364656586762"
export const activeGuildIdAtom = atom<string>({
  key: "activeGuildId",
  default: defaultGuildId,
  effects: [synchronizeWithLocalStorage("activeGuildId")],
})

export function useActiveGuildId() {
  const [activeGuildId, setActiveGuildId] = useRecoilState(activeGuildIdAtom)

  return { activeGuildId, setActiveGuildId }
}
