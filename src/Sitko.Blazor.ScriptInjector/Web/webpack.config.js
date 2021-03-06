const path = require('path');
const TerserPlugin = require('terser-webpack-plugin');

const config = {
  entry: {
    inject: path.resolve(__dirname, 'src', 'inject.ts'),
  },
  module: {
    rules: [
      {
        test: /\.tsx?$/,
        use: 'ts-loader',
        exclude: /node_modules/,
      },
    ],
  },
  resolve: {
    extensions: ['.tsx', '.ts', '.js'],
  },
  output: {
    filename: '[name].js',
    path: path.resolve(__dirname, '..', 'dist'),
  }
};
module.exports = (env, argv) => {
  if (argv.mode === 'development') {
    config.devtool = 'source-map';
  }

  if (argv.mode === 'production') {
    config.optimization = {
      minimize: true,
      minimizer: [
        new TerserPlugin({
          terserOptions: {
            compress: {
              drop_console: true, // will remove console.logs from your files
            },
          },
        }),
      ],
    }
  }

  return config;
};
