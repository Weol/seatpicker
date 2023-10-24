import * as React from 'react';
import {Box, Stack, styled, Tooltip, tooltipClasses, TooltipProps, Typography} from '@mui/material';
import DiscordAvatar from './DiscordAvatar';
import User from "../Models/User";
import Seat from "../Adapters/Models/Seat";

interface SeatProperties {
  seat: Seat;
  color: string;
  onClick: (seat: Seat) => void;
}

export default function SeatComponent(props: SeatProperties) {
  const HtmlTooltip = styled(({className, ...props}: TooltipProps) => (
    <Tooltip {...props} classes={{popper: className}}/>
  ))(({theme}) => ({
    [`& .${tooltipClasses.tooltip}`]: {
      color: theme.palette.text.primary,
      backgroundColor: theme.palette.background.paper,
      boxShadow: 4,
      elevation: 4,
      maxWidth: 220,
      fontSize: theme.typography.pxToRem(12),
    },
  }));

  const r = (): number => {
    const a = 100
    return (300 + (Math.random() * a / 2 + a))
  }

  const q = (): number => {
    const a = 10
    return (20 * Math.random())
  }

  const renderSeat = (seat: Seat) => {
    return (
      <Box key={seat.id} onClick={() => props.onClick(seat)} sx={{
        position: "absolute",
        minWidth: "0",
        top: seat.bounds.y + "%",
        left: seat.bounds.x + "%",
        width: seat.bounds.width + "%",
        height: seat.bounds.height + "%",
        border: "1px #ffffff61 solid",
        backgroundColor: props.color,
        borderTopLeftRadius: r() + "px " + q() + "px",
        borderTopRightRadius: q() + "px " + r() + "px",
        borderBottomRightRadius: r() + "px " + q() + "px",
        borderBottomLeftRadius: q() + "px " + r() + "px",
        cursor: "pointer",
        textAlign: "center",
        display: "flex"
      }}>
        <Typography variant="subtitle1" gutterBottom component="p"
                    sx={{lineHeight: 1, fontSize: "0.9rem", margin: "auto"}}>
          {seat.title}
        </Typography>
      </Box>
    )
  }
  const renderTooltipSeat = (seat: Seat, user: User) => (
    <HtmlTooltip
      title={
        <React.Fragment>
          <Stack direction="row" spacing={1}>
            <DiscordAvatar user={user} style={{width: '25px', height: '25px', borderRadius: '50%'}}/>
            <Typography color="inherit">{seat.reservedBy?.nick}</Typography>
          </Stack>
        </React.Fragment>
      }
    >
      {renderSeat(props.seat)}
    </HtmlTooltip>
  )

  return (props.seat.reservedBy && renderTooltipSeat(props.seat, props.seat.reservedBy) || renderSeat(props.seat))
}
