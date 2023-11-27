import { useEffect, useState } from "react"
import useApiRequests from "./ApiRequestHook"
import { Role } from "./AuthenticationAdapter"
import useVersionId from "./VersionIdHook"

export interface Guild {
  id: string
  name: string
  icon: string | null
}

export interface GuildRoleMapping {
  roleId: string | null
  role: Role | null
}

export interface GuildRole {
  id: string
  name: string
  color: number
}

export function useGuilds(
  sideEffects?: ((guilds: Guild[]) => Promise<void> | void)[]
) {
  const { apiRequest } = useApiRequests()
  const { versionId } = useVersionId("guilds")
  const [guilds, setGuilds] = useState<Guild[] | null>(null)

  useEffect(() => {
    loadGuilds()
  }, [versionId])

  async function loadGuilds(): Promise<Guild[]> {
    const response = await apiRequest("GET", `guild`)

    const guilds = (await response.json()) as Guild[]
    setGuilds(guilds)
    sideEffects?.forEach(async (sideEffect) => await sideEffect(guilds))
    return guilds
  }

  return guilds
}

export function useGuildRoles(guildId: string) {
  const { apiRequest } = useApiRequests()
  const { versionId } = useVersionId("guild_roles_" + guildId)
  const [roleMappings, setRoleMappings] = useState<GuildRoleMapping[] | null>(
    null
  )
  const [roles, setRoles] = useState<GuildRole[] | null>(null)

  useEffect(() => {
    loadRoles()
    loadRoleMapping()
  }, [versionId, guildId])

  const loadRoles = async () => {
    const response = await apiRequest("GET", `guild/${guildId}/roles`)

    const roles = (await response.json()) as GuildRole[]
    setRoles(roles)
  }

  const loadRoleMapping = async () => {
    const response = await apiRequest("GET", `guild/${guildId}/roles/mapping`)

    const roleMappings = (await response.json()) as GuildRoleMapping[]
    setRoleMappings(roleMappings)
  }

  return { roles, roleMappings }
}

export function useGuildAdapter() {
  const { apiRequest } = useApiRequests()

  const setGuildRoleMapping = async (
    guild: Guild,
    mappings: { roleId: string; role: Role }[]
  ) => {
    return await apiRequest("PUT", `guild/${guild.id}/roles/mapping`, mappings)
  }

  return { setGuildRoleMapping }
}
