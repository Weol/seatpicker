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
  roleId: string
  role: Role
}

export interface GuildRole {
  id: string
  name: string
  color: number
}

export function useGuild(guildId: string) {
  const { apiRequest } = useApiRequests()
  const { versionId } = useVersionId("guild_" + guildId)
  const [guild, setGuild] = useState<Guild | "loading" | "not found">("loading")

  useEffect(() => {
    loadGuild()
  }, [versionId])

  async function loadGuild(): Promise<Guild> {
    const response = await apiRequest("GET", `guild/${guildId}`)

    const guild = (await response.json()) as Guild
    setGuild(guild)
    return guild
  }

  return guild
}

export function useGuilds() {
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
    return guilds
  }

  return guilds
}

export function useGuildRoles(guildId: string) {
  const { apiRequest } = useApiRequests()
  const { versionId, invalidate } = useVersionId("guild_roles_" + guildId)
  const [roleMappings, setRoleMappingsState] = useState<
    GuildRoleMapping[] | null
  >(null)
  const [guildRoles, setGuildRoles] = useState<GuildRole[] | null>(null)

  useEffect(() => {
    loadRoles()
    loadRoleMappings()
  }, [versionId, guildId])

  const loadRoles = async () => {
    const response = await apiRequest("GET", `guild/${guildId}/roles`)

    const roles = (await response.json()) as GuildRole[]
    setGuildRoles(roles)
  }

  const loadRoleMappings = async () => {
    const response = await apiRequest("GET", `guild/${guildId}/roles/mapping`)

    const roleMappings = (await response.json()) as GuildRoleMapping[]
    setRoleMappingsState(roleMappings)
  }

  const setRoleMappings = async (
    guild: Guild,
    mappings: GuildRoleMapping[]
  ) => {
    await apiRequest("PUT", `guild/${guild.id}/roles/mapping`, mappings)
    invalidate()
  }

  return { guildRoles, roleMappings, setRoleMappings }
}
