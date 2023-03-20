import * as React from 'react';
import Config from "../config";
import User from '../Models/User';

interface DiscordAvatarProperties {
  user: User;
  style: React.CSSProperties;
}

export default function DiscordAvatar(props: DiscordAvatarProperties) {
  return (<img src={Config.DiscordAvatarBaseUrl + props.user.id + "/" + props.user.avatar} style={props.style}/>)
}
