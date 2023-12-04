import { atom, atomFamily, selector, useRecoilState, useRecoilValue } from "recoil"
import ApiRequest from "./ApiRequestAdapter"
import { Role, User } from "./AuthAdapter"
import { synchronizeWithLocalStorage } from "./Utils"

export type Guild = {
  id: string
  name: string
  icon: string | null
}

export type GuildRole = {
  id: string
  name: string
  color: number
  roles: Role[]
}

export const activeGuildIdAtom = atom<string>({
  key: "activeGuildId",
  default: "65401637126026412",
  effects: [synchronizeWithLocalStorage("activeGuildId")],
})

export const guildAtomFamily = atomFamily({
  key: "guilds",
  default: async (id: string) => {
    const response = await ApiRequest("GET", `guild/${id}`)

    const guild = response.status == 404 ? null : ((await response.json()) as Guild)

    return guild
  },
})

export function useGuild(guildId: string) {
  const guild = useRecoilValue(guildAtomFamily(guildId))

  return guild
}

export const allGuildsAtom = atom({
  key: "allGuilds",
  default: async () => {
    const response = await ApiRequest("GET", `guild`)

    const guilds = (await response.json()) as Guild[]

    return guilds
  },
})

export function useGuilds() {
  const guilds = useRecoilValue(allGuildsAtom)

  return guilds
}

export const guildUsersSelector = selector({
  key: "guildUsers",
  get: async ({ get }) => {
    const activeGuildId = get(activeGuildIdAtom)

    const response = await ApiRequest("GET", `guild/${activeGuildId}/users`)

    const users = response.status == 404 ? null : ((await response.json()) as User[])

    return users
  },
})

export function useGuildUsers() {
  const guildUsers = useRecoilValue(guildUsersSelector)

  return guildUsers
}

export const guildRolesAtomFamily = atomFamily({
  key: "guildRoles",
  default: async (id: string) => {
    const response = await ApiRequest("GET", `guild/${id}/roles`)

    return (await response.json()) as GuildRole[]
  },
})

export function useGuildRoles(guildId: string) {
  const [guildRoles, setGuildRoles] = useRecoilState(guildRolesAtomFamily(guildId))

  const reloadGuildRoles = async () => {
    const response = await ApiRequest("GET", `guild/${guildId}/roles`)

    const roles = (await response.json()) as GuildRole[]
    setGuildRoles(roles)
  }

  const updateGuildRoles = async (guild: Guild, roles: GuildRole[]) => {
    await ApiRequest("PUT", `guild/${guild.id}/roles/mapping`, roles)
    await reloadGuildRoles()
  }

  return { guildRoles, updateGuildRoles }
}
