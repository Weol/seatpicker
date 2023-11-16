import * as React from "react"
import { Stack } from "@mui/material"
import Typography from "@mui/material/Typography"
import { useEffect, useState } from "react"
import DelayedCircularProgress from "../Components/DelayedCircularProgress"
import { GuildRoleMapping, useGuildAdapter } from "../Adapters/GuildAdapter"
import { useParams } from "react-router-dom"

export function RoleMapping() {
  const { guildId } = useParams()
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const { guilds, getGuildRoleMapping, setGuildRoleMapping } = useGuildAdapter()
  const [roleMapping, setRoleMapping] = useState<GuildRoleMapping[] | null>(
    null
  )
  const guild = guilds?.find((guild) => guild.id == guildId, null) ?? null

  useEffect(() => {
    if (guildId != undefined) {
      // eslint-disable-next-line promise/catch-or-return
      getGuildRoleMapping(guildId).then((roleMapping) =>
        setRoleMapping(roleMapping)
      )
    }
  }, [guildId])

  return (
    <Stack>
      <Typography>{guild?.name}</Typography>
      {roleMapping ? (
        roleMapping.map((mapping) => (
          <Typography>{mapping.roleName}</Typography>
        ))
      ) : (
        <DelayedCircularProgress />
      )}
    </Stack>
  )
}
