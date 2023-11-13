import * as React from "react"
import { Box, Container, Typography } from "@mui/material"
import { SeatMenu } from "./SeatMenu"
import { Seat } from "../Adapters/SeatsAdapter"

interface SeatProperties {
  seat: Seat
  color: string
  onClick: (seat: Seat) => void
}

export default function SeatComponent(props: SeatProperties) {
  const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null)
  const open = Boolean(anchorEl)

  const handleClick = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget)
    // props.onClick(props.seat)
  }

  const handleClose = () => {
    setAnchorEl(null)
  }

  const renderSeat = (seat: Seat) => {
    return (
      <Container>
        <Box
          key={seat.id}
          onClick={handleClick}
          sx={{
            position: "absolute",
            minWidth: "0",
            top: seat.bounds.y + "%",
            left: seat.bounds.x + "%",
            width: seat.bounds.width + "%",
            height: seat.bounds.height + "%",
            border: "1px #ffffff61 solid",
            backgroundColor: props.color,
            cursor: "pointer",
            textAlign: "center",
            display: "flex",
          }}
        >
          <Typography
            variant="subtitle1"
            gutterBottom
            component="p"
            sx={{ lineHeight: 1, fontSize: "0.9rem", margin: "auto" }}
          >
            {seat.title}
          </Typography>
        </Box>
        <SeatMenu
          open={open}
          seat={seat}
          id="demo-positioned-menu"
          aria-labelledby="demo-positioned-button"
          anchorEl={anchorEl}
          onClose={handleClose}
          anchorOrigin={{
            vertical: "top",
            horizontal: "center",
          }}
          transformOrigin={{
            vertical: "bottom",
            horizontal: "center",
          }}
        />
      </Container>
    )
  }

  return renderSeat(props.seat)
}
