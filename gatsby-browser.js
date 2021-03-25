import * as React from "react";
import Layout from "./src/layouts";

export const wrapRootElement = ({ element }) => {
  return <Layout>{element}</Layout>;
};
