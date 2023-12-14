import type { Meta, StoryObj } from "@storybook/react";

import { Header } from "./index";
import { Refine } from "@refinedev/core";
import dataProvider from "@refinedev/simple-rest";

const meta = {
  title: "components/header",
  component: Header,
  parameters: {
    layout: "fullscreen",
  },
  decorators: [
    (story) => <Refine dataProvider={dataProvider("")}> {story()} </Refine>,
  ],
} satisfies Meta<typeof Header>;

export default meta;
type Story = StoryObj<typeof meta>;

export const Default: Story = {
  args: {},
};
