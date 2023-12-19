import { Edit } from "@refinedev/mui";
import { Box, TextField } from "@mui/material";
import { useForm } from "@refinedev/react-hook-form";
import {
  HttpError,
  IResourceComponentsProps,
  useTranslate,
} from "@refinedev/core";
import { IUser, Nullable } from "../../interfaces";

export const UserEdit: React.FC<IResourceComponentsProps> = () => {
  const translate = useTranslate();
  const {
    saveButtonProps,
    register,
    formState: { errors },
  } = useForm<
    IUser & { newPassword: string },
    HttpError,
    Nullable<IUser & { newPassword: string }>
  >();

  return (
    <Edit saveButtonProps={saveButtonProps}>
      <Box
        component="form"
        sx={{ display: "flex", flexDirection: "column" }}
        autoComplete="off"
      >
        <TextField
          {...register("name", {
            required: "This field is required",
          })}
          error={!!errors?.name}
          helperText={errors?.name?.message}
          margin="normal"
          fullWidth
          InputLabelProps={{ shrink: true }}
          type="text"
          label={translate("users.fields.name")}
          name="name"
        />
        <TextField
          {...register("username", {
            required: "This field is required",
          })}
          error={!!errors?.username}
          helperText={errors?.username?.message}
          margin="normal"
          fullWidth
          InputLabelProps={{ shrink: true }}
          type="text"
          label={translate("users.fields.username")}
          name="username"
        />
        <TextField
          {...register("emailAddress", {
            required: "This field is required",
          })}
          error={!!errors?.emailAddress}
          helperText={errors?.emailAddress?.message}
          margin="normal"
          fullWidth
          InputLabelProps={{ shrink: true }}
          type="email"
          label={translate("users.fields.emailAddress")}
          name="emailAddress"
        />
        <TextField
          {...register("newPassword")}
          error={!!errors?.newPassword}
          helperText={errors?.newPassword?.message}
          margin="normal"
          fullWidth
          InputLabelProps={{ shrink: true }}
          type="password"
          label={translate("users.fields.newPassword")}
          name="newPassword"
        />
        <TextField
          {...register("role", {
            required: "This field is required",
          })}
          error={!!errors?.role}
          helperText={errors?.role?.message}
          margin="normal"
          fullWidth
          InputLabelProps={{ shrink: true }}
          type="text"
          label={translate("users.fields.role")}
          name="role"
        />
      </Box>
    </Edit>
  );
};
