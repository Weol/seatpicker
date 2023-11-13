import { Avatar, AvatarProps } from "@mui/material"
import * as React from "react"
import Config from "../config"
import { User } from "../Adapters/AuthenticationAdapter"

interface DiscordAvatarProperties {
  user: User
}

export default function DiscordAvatar(
  props: DiscordAvatarProperties & AvatarProps
) {
  return (
    <Avatar
      alt={props.user.name}
      src={
        Config.DiscordAvatarBaseUrl + props.user.id + "/" + props.user.avatar
      }
      {...props}
    />
  )
}
