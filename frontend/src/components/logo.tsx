import Typography from "@mui/material/Typography";
import React from "react";

type LogoProps = {
  size?: "sm" | "md" | "lg";
};

export const Logo: React.FC<LogoProps> = ({ size = "md" }) => {
  return (
    <Typography
      variant={
        { sm: "h4" as const, md: "h3" as const, lg: "h2" as const }[size]
      }
    >
      Trsys
    </Typography>
  );
};
