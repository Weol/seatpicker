import { useEffect } from "react"
import useApiRequests from "./ApiRequestHook"
import { Role } from "./AuthenticationAdapter"
import { useApi } from "../Contexts/ApiContext"

export interface Guild {
  id: string
  name: string
  icon: string | null
}

export interface GuildRoleMapping {
  roleId: string
  roleName: string
  roleColor: number
  roleIcon?: string
  role: Role | null
}

export function useGuildAdapter() {
  const { apiRequest } = useApiRequests()
  const { guilds, setGuilds } = useApi()

  useEffect(() => {
    if (guilds == null) {
      reloadGuilds()
    }
  }, [])

  const reloadGuilds = async (): Promise<Guild[]> => {
    const response = await apiRequest("GET", `discord/guilds`)

    const guilds = (await response.json()) as Guild[]
    setGuilds(guilds)
    return guilds
  }

  const getGuildRoleMapping = async (guildId: string) => {
    const response = await apiRequest("GET", `discord/guild/${guildId}/roles`)

    return (await response.json()) as GuildRoleMapping[]
  }

  const setGuildRoleMapping = async (
    guild: Guild,
    mappings: { roleId: string; role: Role }[]
  ) => {
    return await apiRequest("PUT", `discord/guild/${guild.id}/roles`, mappings)
  }

  return { guilds, getGuildRoleMapping, setGuildRoleMapping }
}
