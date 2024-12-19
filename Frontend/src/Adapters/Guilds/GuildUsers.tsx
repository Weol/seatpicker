import { atomFamily, useRecoilValue } from "recoil"
import { ApiRequest } from "../ApiRequest"
import { User } from "../Models"

async function LoadGuildUsers(guildId: string) {
  const response = await ApiRequest("GET", `guild/${guildId}/users`)
  return (await response.json()) as User[]
}

export const guildUsersAtomFamily = atomFamily<User[], string>({
  key: "guildUsers",
  default: LoadGuildUsers,
})

export function useGuildUsers(guildId: string) {
  return useRecoilValue(guildUsersAtomFamily(guildId))
}
